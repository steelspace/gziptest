using System;
using System.IO;
using System.IO.Compression;

namespace GZipTest.Zipper
{
    public class Chunk
    {
        public byte[] Input { get; set; }
        public byte[] Result { get; set; }
        public long Offset { get; set; }
        public int Size { get; set; }
        public bool HasResult { get; set; }

        public Chunk() { }

        public Chunk(long offset, int size)
        {
            Offset = offset;
            Size = size;
        }

        public void Compress()
        {
            using (var zippedMemoryStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(zippedMemoryStream, CompressionMode.Compress, true))
                {
                    zipStream.Write(Input, 0, Input.Length);
                }

                var resultLength = BitConverter.GetBytes(zippedMemoryStream.Length);
                Result = new byte[resultLength.Length + zippedMemoryStream.Length];
                resultLength.CopyTo(Result, 0);
                zippedMemoryStream.ToArray().CopyTo(Result, resultLength.Length);
            }

            HasResult = true;
        }

        public void Decompress()
        {
            const int bufferSize = 9 * 1024;

            using (var unzippedMemoryStream = new MemoryStream())
            {
                using (var zippedMemoryStream = new MemoryStream(Input))
                using (var zipStream = new GZipStream(zippedMemoryStream, CompressionMode.Decompress))
                {
                    var buffer = new byte[bufferSize];

                    int read;
                    while ((read = zipStream.Read(buffer, 0, bufferSize)) != 0)
                    {
                        unzippedMemoryStream.Write(buffer, 0, read);
                    }
                }

                Result = unzippedMemoryStream.ToArray();
            }

            HasResult = true;
        }

        public void WriteResult(Stream targetStream)
        {
            if (Result == null)
            {
                return;
            }

            targetStream.Write(Result, 0, Result.Length);
            targetStream.Flush();
        }
    }
}