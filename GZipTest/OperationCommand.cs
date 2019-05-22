using System;
using System.IO;

namespace GZipTest
{
    public class OperationCommand
    {
        const string WrongParamaters = "Missing paramneters, usage: gziptest compress|decompress source-file destination-path.";

        public enum OperationKind
        {
            Compress,
            Decompress
        }

        public OperationKind Operation { get; private set; } = OperationKind.Compress;

        public string SourceFilePath { get; private set; }
        public string TargetFilePath { get; private set; }

        public void ProcessCommandParameters(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ApplicationException(WrongParamaters);
            }

            OperationKind operation;

            if (Enum.TryParse(args[0], true, out operation))
            {
                Operation = operation;
            }
            else
            {
                throw new ApplicationException($"This command is unknown. Use '{OperationKind.Compress}' or '{OperationKind.Decompress}'.");
            }

            SourceFilePath = args[1];
            TargetFilePath = args[2];

            if (!File.Exists(SourceFilePath))
            {
                throw new ApplicationException("Source file does not exist.");
            }

            if (!Directory.Exists(Path.GetDirectoryName(TargetFilePath)))
            {
                throw new ApplicationException("Target path does not exist.");
            }
        }
    }
}