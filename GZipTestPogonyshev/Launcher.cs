using System;
using System.Collections.Generic;
using System.Linq;

using GZipTestPogonyshev.Converters;
using GZipTestPogonyshev.InitialData;
using GZipTestPogonyshev.Validation;

namespace GZipTestPogonyshev {

    internal static class Launcher {

        private static int Main(string[] args) {
            ShowInfo();
            AddUnhandledExceptionHandler();
#if DEBUG
            args = new[] {
                "decompress",
                "D:/Downloads/TorrentDownloads/234.gz",
                "D:/Downloads/TorrentDownloads/234.zip"
            };
#endif
            IReadOnlyList<string> errors = ValidationInput.ValidateInput(args);

            if (errors.Any()) {
                string errorOutput = string.Join("\n", errors);
                Console.WriteLine(errorOutput);
                return Constants.ErrorExitCode;
            }

            string stringOperationType = args[0];
            string sourceFile = args[1];
            string destinationFile = args[2];

            var operationParameters = new OperationParameters(sourceFile, destinationFile, stringOperationType);

            try {
                OperationType operationType = new StringToOperationTypeConverter().GetOperationType(operationParameters.Type.ToLower());
                int threadsLimit = Environment.ProcessorCount;

                BaseOperation operation;
                switch (operationType) {
                    case OperationType.Compress:
                        operation = new Compression(operationParameters, threadsLimit);
                        break;
                    case OperationType.Decompress:
                        operation = new Decompression(operationParameters, threadsLimit);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(operationType));
                }


                return operation.Start();
            } catch (System.Exception ex) {
                Console.Error.WriteLine(ex.Message);
                return Constants.ErrorExitCode;
            }
        }

        private static void ShowInfo() {
            Console.WriteLine(
                "To zip or unzip files please proceed with the following pattern to type in:\n" +
                "Zipping: compress [Source file path] [Destination file path]\n" +
                "Unzipping: decompress [Compressed file path] [Destination file path]\n");
        }

        private static void AddUnhandledExceptionHandler() {
            AppDomain.CurrentDomain.UnhandledException += (o, e) => { Console.Error.WriteLine(e); };
        }

    }

}