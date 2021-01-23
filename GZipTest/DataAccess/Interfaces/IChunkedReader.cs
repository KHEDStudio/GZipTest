using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.DataAccess.Interfaces
{
    public interface IChunkedReader
    {
        public event Action<Chunk, bool> ChunkRead;
        public long ReadAllBytes();
    }
}
