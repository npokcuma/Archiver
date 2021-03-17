using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

using GZipTestPogonyshev.Exception;
using GZipTestPogonyshev.InitialData;
using GZipTestPogonyshev.Model;

namespace GZipTestPogonyshev {

    internal class Decompression : BaseOperation {

        public Decompression(OperationParameters parameters, int threadsLimit) : base(parameters, threadsLimit) {
            long initialLength = br.ReadInt64();
            long blockCount = br.ReadInt64();

            FileInformation = new FileInformation(initialLength, blockCount);

            CheckDiskSpace(parameters.DestinationFile);
        }

        private void CheckDiskSpace(string destinationFile) {
            var fi = new FileInfo(destinationFile);
            if (fi.Directory != null) {
                var drive = new DriveInfo(fi.Directory.Root.FullName);
                if (drive.DriveFormat == "FAT32" && FileInformation.InitialLength > FileInformation.Fat32MaxFileSize) {
                    throw new IOException("Not enough disk space to write the unpacked file (FAT32 limitation)");
                }
            }
        }

        protected override Result<BlockCollection> Read(long leftBlocksCount) {
            try {
                var blocks = new List<Block>();
                var readerCollection = new BlockCollection();
                do {
                    int number = br.ReadInt32();
                    int length = br.ReadInt32();
                    byte[] value = br.ReadBytes(length);

                    var block = new Block(number, value.ToArray());

                    blocks.Add(block);
                } while (blocks.Count != Math.Min(threadsLimit * Constants.MegabytesForReading, leftBlocksCount));

                readerCollection.SetBlocks(blocks);
                blocks.Clear();

                return Result.Ok(readerCollection);
            } catch (System.Exception ex) {
                return Result.Fail<BlockCollection>(StreamException.GetErrorText(ex));
            }
        }

        protected override Result<BlockCollection> ZipConvert(BlockCollection readerCollection) {
            int bufferSize = FileInformation.BufferSize;
            int blocksCount = readerCollection.Blocks.Count;
            int chunks = blocksCount / threadsLimit;

            var threads = new Thread[threadsLimit];
            var decompressedCollection = new BlockCollection();
            var errors = new List<string>();

            try {
                for (var i = 0; i < threadsLimit; i++) {
                    long chunkStart = i * chunks;
                    long chunkEnd = chunkStart + chunks;
                    if (i == threadsLimit - 1) {
                        chunkEnd += blocksCount % threadsLimit;
                    }

                    threads[i] = new Thread(
                        () => {
                            Result<IReadOnlyList<Block>> blocksResult = GetDecompressedBlocksByChunkResult(chunkStart, chunkEnd, bufferSize, readerCollection);

                            if (blocksResult.Success) {
                                decompressedCollection.SetBlocks(blocksResult.Value);
                            } else {
                                Monitor.Enter(mLock);
                                errors.Add(blocksResult.Error);
                                Monitor.Exit(mLock);
                            }
                        }) {
                        IsBackground = true,
                        Priority = ThreadPriority.AboveNormal
                    };

                    threads[i].Start();
                }

                foreach (Thread thread in threads) {
                    thread.Join();
                }

                return errors.Any()
                    ? Result.Fail<BlockCollection>(string.Join("\n", errors))
                    : Result.Ok(decompressedCollection.GetSortedCollection());
            } catch (System.Exception ex) {
                return Result.Fail<BlockCollection>(StreamException.GetErrorText(ex));
            }
        }

        private static Result<IReadOnlyList<Block>> GetDecompressedBlocksByChunkResult(long chunkStart, long chunkEnd, int bufferSize, BlockCollection readerCollection) {
            try {
                var blocks = new List<Block>();
                for (long number = chunkStart; number < chunkEnd; ++number) {
                    var decompressedBlock = new byte[bufferSize];
                    Block block = readerCollection.Blocks.ElementAt((int)number);
                    int size;
                    using (var compressedBlock = new MemoryStream(block.Buffer.ToArray())) {
                        using (var zip = new GZipStream(compressedBlock, CompressionMode.Decompress)) {
                            size = zip.Read(decompressedBlock, 0, bufferSize);
                        }
                    }

                    var newBlock = new Block(block.Number, decompressedBlock.Take(size).ToArray());
                    blocks.Add(newBlock);
                }

                return Result.Ok<IReadOnlyList<Block>>(blocks);
            } catch (System.Exception ex) {
                return Result.Fail<IReadOnlyList<Block>>(StreamException.GetErrorText(ex));
            }
        }

        protected override Result Write(BlockCollection zipCollection) {
            try {
                foreach (Block block in zipCollection.Blocks) {
                    bw.Write(block.Buffer);
                }

                return Result.Ok();
            } catch (System.Exception ex) {
                return Result.Fail(StreamException.GetErrorText(ex));
            }
        }

    }

}