namespace GZipTestPogonyshev
{
    internal class OperationParameters {

        public OperationParameters(string sourceFile, string destinationFile, string operationType) {
            Type = operationType;
            SourceFile = sourceFile;
            DestinationFile = destinationFile;
        }

        public string SourceFile { get; }

        public string DestinationFile { get; }

        public string Type { get; }
    }
}