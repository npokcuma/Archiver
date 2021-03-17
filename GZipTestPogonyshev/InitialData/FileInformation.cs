
namespace GZipTestPogonyshev.InitialData 
{
    internal class FileInformation {

        public FileInformation(string sourceFile) {
            var fi = new System.IO.FileInfo(sourceFile);
            InitialLength = fi.Length;
            BlockCount = fi.Length / BufferSize;

            if (fi.Length % BufferSize > 0) {
                BlockCount++;
            }
        }

        public FileInformation(long initialLength, long blockCount) {
            InitialLength = initialLength;
            BlockCount = blockCount;
        }

        public long BlockCount { get; }

        public long InitialLength { get; }

        public static int BufferSize => 1048576;

        public static long Fat32MaxFileSize => 4294967295;

    }
}
