﻿using GZipTest.Tasks;
using GZipTest.Zipper;
using System;
using System.IO;

namespace GZipTest
{
    public class Compressor
    {
        private static Exception exception;

        public void Compress(string sourceFile, string targetFile)
        {
            Execute(sourceFile, targetFile, true);

            Console.WriteLine($"Finished compressing file to {targetFile}");
        }

        public void Decompress(string sourceFile, string targetFile)
        {
            Execute(sourceFile, targetFile, false);

            Console.WriteLine($"Finished decompressing file to {targetFile}");
        }

        private void Execute(string sourceFile, string targetFile, bool compress)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open))
            using (var targetStream = new FileStream(targetFile, FileMode.Create))

            using (var zipper = new Zipper.Zipper(sourceStream, targetStream, new Queue(ThreadExceptionHandler)))
            {
                if (compress)
                {
                    zipper.Compress();
                }
                else
                {
                    zipper.Decompress();
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        private void ThreadExceptionHandler(Exception exception)
        {
            Compressor.exception = exception;
        }
    }
}
