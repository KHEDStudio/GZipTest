using System;
using System.Collections.Generic;
using System.Text;

namespace GZipTest.Shared.Models
{
    public class Chunk
    {
        public int Index { get; private set; }
        public byte[] Bytes { get; private set; }

        public Chunk(int index, byte[] bytes)
        {
            Index = index;
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }
    }
}
