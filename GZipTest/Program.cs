using System;
using static GZipTest.OperationCommand;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandLineParameters = new OperationCommand();
            var compressor = new Compressor();

            try
            {
                commandLineParameters.ProcessCommandParameters(args);

                switch (commandLineParameters.Operation)
                {
                    case OperationKind.Compress:
                        {
                            compressor.Compress(commandLineParameters.SourceFilePath, commandLineParameters.TargetFilePath);
                            break;
                        }
                    case OperationKind.Decompress:
                        {
                            compressor.Decompress(commandLineParameters.SourceFilePath, commandLineParameters.TargetFilePath);
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException($"Unsupported command {commandLineParameters.Operation}.");
                        }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
