using System;
using System.Collections.Generic;
using System.Text;

namespace GZipTest.DataAccess.Interfaces
{
    public interface ISizeChunkReader
    {
        public byte[] GetChunkBytes(int offset);
    }
}
