using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GZipTest.Tasks;

namespace GZipTest.Zipper
{
    public class Zipper : IDisposable
    {
        private const uint ChunkBufferSize = 1204 * 1024;

        private readonly Stream _sourceStream;
        private readonly Stream _targetStream;

        private readonly Queue _queue;

        private List<Chunk> _chunks;

        public void Compress()
        {
            Zip(_sourceStream, _targetStream, ChunkBufferSize);
            _queue.StartThreads();
        }

        public void Decompress()
        {
            Unzip(_sourceStream, _targetStream);
            _queue.StartThreads();
        }

        private void Zip(Stream sourceStream, Stream targetStream, long bufferSize)
        {
            var chunkNumber = Math.Ceiling((double)sourceStream.Length / bufferSize);

            if (bufferSize > sourceStream.Length)
            {
                bufferSize = sourceStream.Length;
            }

            for (int chunkIndex = 0; chunkIndex < chunkNumber; chunkIndex++)
            {
                var chunk = new Chunk();

                int index = chunkIndex;
                long size = bufferSize;

                long offset = index * size;

                if (offset + size > sourceStream.Length)
                {
                    size = sourceStream.Length - offset;
                }

                chunk.Offset = offset;

                var compressingTask = new MyTask(() =>
                {
                    var chunkArray = new byte[size];
                    lock (sourceStream)
                    {
                        sourceStream.Seek(chunk.Offset, SeekOrigin.Begin);
                        sourceStream.Read(chunkArray, 0, (int)size);
                    }

                    chunk.Input = chunkArray;
                    chunk.Compress();
                });

                _queue.CompressTasks.Enqueue(compressingTask);

                var writingTask = new MyTask(() =>
                {
                    while (true)
                    {
                        if (!chunk.HasResult)
                        {
                            continue;
                        }

                        lock (targetStream)
                        {
                            chunk.WriteResult(targetStream);
                        }

                        break;
                    }
                });

                _queue.WritingTasks.Enqueue(writingTask);
            }
        }

        private List<Chunk> GetChunks(Stream sourceStream)
        {
            if (_chunks != null && _chunks.Any())
            {
                return _chunks;
            }

            long offset = 0;
            var chunkSizeBufferLength = sizeof(long);
            var chunkSizeBuffer = new byte[chunkSizeBufferLength];

            var chunks = new List<Chunk>();

            while (offset + chunkSizeBufferLength < sourceStream.Length)
            {
                sourceStream.Seek(offset, SeekOrigin.Begin);
                sourceStream.Read(chunkSizeBuffer, 0, chunkSizeBufferLength);

                var chunkBufferLength = BitConverter.ToInt32(chunkSizeBuffer, 0);
                offset += chunkSizeBufferLength;

                var chunk = new Chunk(offset, chunkBufferLength);
                chunks.Add(chunk);

                offset += chunkBufferLength;
            }

            return chunks;
        }

        private void Unzip(Stream sourceStream, Stream targetStream)
        {
            _chunks = GetChunks(sourceStream);

            foreach (var chunk in _chunks)
            {
                var decompressingTask = new MyTask(() =>
                {
                    lock (sourceStream)
                    {
                        sourceStream.Seek(chunk.Offset, SeekOrigin.Begin);
                        chunk.Input = new byte[chunk.Size];
                        sourceStream.Read(chunk.Input, 0, chunk.Size);
                    }

                    chunk.Decompress();
                });

                _queue.CompressTasks.Enqueue(decompressingTask);

                var writingTask = new MyTask(() =>
                {
                    while (true)
                    {
                        if (!chunk.HasResult)
                        {
                            continue;
                        }

                        lock (targetStream)
                        {
                            chunk.WriteResult(targetStream);
                        }

                        break;
                    }
                });

                _queue.WritingTasks.Enqueue(writingTask);
            }
        }

        public Zipper(Stream sourceStream, Stream targetStream, Queue queue)
        {
            _sourceStream = sourceStream;
            _targetStream = targetStream;
            _queue = queue;
        }

        public void Dispose()
        {
            _sourceStream?.Dispose();
            _targetStream?.Dispose();
        }
    }
}