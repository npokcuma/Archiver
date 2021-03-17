using System;
using System.IO;

using GZipTestPogonyshev.InitialData;
using GZipTestPogonyshev.Model;

namespace GZipTestPogonyshev {

    internal abstract class BaseOperation {

        protected readonly int threadsLimit;

        protected readonly BinaryReader br;
        protected readonly BinaryWriter bw;

        protected readonly object mLock = new object();

        protected BaseOperation(OperationParameters parameters, int threadsLimit) {
            this.threadsLimit = threadsLimit;

            br = new BinaryReader(new FileStream(parameters.SourceFile, FileMode.Open, FileAccess.Read, FileShare.None));
            bw = new BinaryWriter(new FileStream(parameters.DestinationFile, FileMode.Create, FileAccess.Write, FileShare.None));
        }

        protected FileInformation FileInformation { get; set; }

        public int Start() {
            long leftBlocksCount = FileInformation.BlockCount;

            while (leftBlocksCount != 0) {
                Result<BlockCollection> readerCollectionResult = Read(leftBlocksCount);

                if (readerCollectionResult.Failure) {
                    return GetErrorExitResult(readerCollectionResult.Error);
                }

                leftBlocksCount -= readerCollectionResult.Value.Blocks.Count;

                Result<BlockCollection> zipCollectionResult = ZipConvert(readerCollectionResult.Value);

                if (zipCollectionResult.Failure) {
                    return GetErrorExitResult(zipCollectionResult.Error);
                }

                readerCollectionResult.Value.ClearCollection();

                Result writerResult = Write(zipCollectionResult.Value);

                if (writerResult.Failure) {
                    return GetErrorExitResult(writerResult.Error);
                }

                zipCollectionResult.Value.ClearCollection();
            }

            DisposeStreams();

            return Constants.SuccessExitCode;
        }

        private int GetErrorExitResult(string error) {
            Console.Error.WriteLine(error);
            DisposeStreams();
            return Constants.ErrorExitCode;
        }

        private void DisposeStreams() {
            br?.Dispose();
            bw?.Dispose();
        }

        protected abstract Result<BlockCollection> Read(long leftBlocksCount);

        protected abstract Result<BlockCollection> ZipConvert(BlockCollection readerCollection);

        protected abstract Result Write(BlockCollection zipCollection);

    }

}