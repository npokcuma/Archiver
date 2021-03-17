using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GZipTestPogonyshev.Model {

    internal class BlockCollection {

        private readonly List<Block> blockCollection;
        private readonly object mLock = new object();

        public BlockCollection() {
            blockCollection = new List<Block>();
        }

        public IReadOnlyList<Block> Blocks => blockCollection.AsReadOnly();

        public void SetBlocks(IReadOnlyList<Block> blocks) {
            if (blocks == null) {
                throw new ArgumentNullException(nameof(blocks));
            }

            Monitor.Enter(mLock);
            blockCollection.AddRange(blocks);
            Monitor.Exit(mLock);
        }

        public void ClearCollection() {
            Monitor.Enter(mLock);
            blockCollection.Clear();
            Monitor.Exit(mLock);
        }

        public BlockCollection GetSortedCollection() {
            var cloneCollection = new BlockCollection();
            List<Block> sortedBlocks = Blocks.OrderBy(block => block.Number).ToList();
            cloneCollection.SetBlocks(sortedBlocks);

            return cloneCollection;
        }

    }

}