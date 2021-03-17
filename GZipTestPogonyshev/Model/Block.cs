using System;

namespace GZipTestPogonyshev.Model {

    internal class Block {

        private byte[] buffer;

        public Block(int number, byte[] value) {
            Number = number;
            buffer = value;
        }

        public int Number { get; }

        public byte[] Buffer {
            get => buffer;
            set => buffer = value ?? throw new ArgumentNullException(nameof(Buffer));
        }

    }

}