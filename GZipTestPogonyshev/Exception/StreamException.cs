using System;
using System.IO;

namespace GZipTestPogonyshev.Exception
{
    internal static class StreamException
    {
        public static string GetErrorText(System.Exception exception) {
            switch (exception) {
                case FileNotFoundException _:
                    return "The file or directory cannot be found.";
                case DirectoryNotFoundException _:
                    return "The file or directory cannot be found.";
                case DriveNotFoundException _:
                    return "The drive specified in 'path' is invalid.";
                case PathTooLongException _:
                    return "'path' exceeds the maximum supported path length.";
                case UnauthorizedAccessException _:
                    return "You do not have permission to create this file.";
                case IOException e when (e.HResult & 0x0000FFFF) == 32:
                    return "There is a sharing violation.";
                case IOException ex when (ex.HResult & 0x0000FFFF) == 80:
                    return "The file already exists.";
                case IOException ioEx:
                    return "An exception occurred:\nError code: " +
                                          $"{ioEx.HResult & 0x0000FFFF}\nMessage: {ioEx.Message}";
                default:
                    return $"Error: {exception.Message}";
            }
        }
    }
}