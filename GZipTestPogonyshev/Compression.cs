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

    internal class Compression : BaseOperation {

        public Compression(OperationParameters parameters, int threadsLimit) : base(parameters, threadsLimit) {
            FileInformation = new FileInformation(parameters.SourceFile);

            bw.Write(BitConverter.GetBytes(FileInformation.InitialLength));
            bw.Write(BitConverter.GetBytes(FileInformation.BlockCount));
        }

        protected override Result<BlockCollection> Read(long leftBlocksCount) {
            try {
                var blocks = new List<Block>();
                var readerCollection = new BlockCollection();
                var blockNumber = 0;
                do {
                    var readBlock = new Block(blockNumber, br.ReadBytes(FileInformation.BufferSize));
                    blocks.Add(readBlock);
                    blockNumber++;
                } while (blocks.Count != Math.Min(threadsLimit * Constants.MegabytesForReading, leftBlocksCount));

                readerCollection.SetBlocks(blocks);
                blocks.Clear();

                return Result.Ok(readerCollection);
            } catch (System.Exception ex) {
                return Result.Fail<BlockCollection>(StreamException.GetErrorText(ex));
            }
        }

        protected override Result<BlockCollection> ZipConvert(BlockCollection readerCollection) {
            int chunks = readerCollection.Blocks.Count / threadsLimit;

            var threads = new Thread[threadsLimit];
            var compressedCollection = new BlockCollection();
            var errors = new List<string>();

            try {
                for (var i = 0; i < threadsLimit; i++) {
                    long chunkStart = i * chunks;
                    long chunkEnd = chunkStart + chunks;
                    if (i == threadsLimit - 1) {
                        chunkEnd += readerCollection.Blocks.Count % threadsLimit;
                    }

                    threads[i] = new Thread(
                        () => {
                            Result<IReadOnlyList<Block>> blocksResult = GetCompressedBlocksByChunkResult(chunkStart, chunkEnd, readerCollection);

                            if (blocksResult.Success) {
                                compressedCollection.SetBlocks(blocksResult.Value);
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
                    : Result.Ok(compressedCollection.GetSortedCollection());
            } catch (System.Exception ex) {
                return Result.Fail<BlockCollection>(StreamException.GetErrorText(ex));
            }
        }

        private static Result<IReadOnlyList<Block>> GetCompressedBlocksByChunkResult(long chunkStart, long chunkEnd, BlockCollection readerCollection) {
            try {
                var blocks = new List<Block>();

                for (long number = chunkStart; number < chunkEnd; ++number) {
                    Block block = readerCollection.Blocks.ElementAt((int)number);
                    using var memoryStream = new MemoryStream();
                    using (var zip = new GZipStream(memoryStream, CompressionMode.Compress)) {
                        zip.Write(block.Buffer, 0, block.Buffer.Length);
                    }

                    blocks.Add(new Block(block.Number, memoryStream.ToArray()));
                }

                return Result.Ok<IReadOnlyList<Block>>(blocks);
            } catch (System.Exception ex) {
                return Result.Fail<IReadOnlyList<Block>>(StreamException.GetErrorText(ex));
            }
        }

        protected override Result Write(BlockCollection zipCollection) {
            try {
                foreach (Block block in zipCollection.Blocks) {
                    bw.Write(BitConverter.GetBytes(block.Number));
                    bw.Write(block.Buffer.Length);
                    bw.Write(block.Buffer.ToArray());
                }

                return Result.Ok();
            } catch (System.Exception ex) {
                return Result.Fail(StreamException.GetErrorText(ex));
            }
        }

    }

}