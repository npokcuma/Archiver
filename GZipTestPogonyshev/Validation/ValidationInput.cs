using System.Collections.Generic;
using System.IO;

namespace GZipTestPogonyshev.Validation {

    internal static class ValidationInput {

        public static IReadOnlyList<string> ValidateInput(string[] args) {
            var errors = new List<string>();

            if (args.Length != 3) {
                errors.Add("Please enter arguments up to the following pattern:\n compress/decompress [Source file] [Destination file].");
                return errors;
            }

            string stringOperationType = args[0].ToLower();
            string source = args[1].ToLower();
            string destination = args[2].ToLower();

            if (string.IsNullOrWhiteSpace(source)) {
                errors.Add("Source file path is empty.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(destination)) {
                errors.Add("Destination file path is empty.");
                return errors;
            }

            if (stringOperationType != "compress" && stringOperationType != "decompress") {
                errors.Add("First argument shall be \"compress\" or \"decompress\".");
            }

            if (!File.Exists(source)) {
                errors.Add("No source file was found.");
            }

            if (!Directory.Exists(Path.GetDirectoryName(destination))) {
                errors.Add("Invalid destination file path.");
            }

            var sourceFile = new FileInfo(source);
            var destinationFile = new FileInfo(destination);

            if (sourceFile == destinationFile) {
                errors.Add("Source and destination files shall be different.");
            }

            if (sourceFile.Extension == ".gz" && stringOperationType == "compress") {
                errors.Add("File has already been compressed.");
            }

            if (destinationFile.Extension == ".gz" && destinationFile.Exists) {
                errors.Add("Destination file already exists.Please indicate the different file name.");
            }

            if (sourceFile.Extension != ".gz" && stringOperationType == "decompress") {
                errors.Add("File to be decompressed shall have .gz extension.");
            }

            return errors;
        }

    }

}