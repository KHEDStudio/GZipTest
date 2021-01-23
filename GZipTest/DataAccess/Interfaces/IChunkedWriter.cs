using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.DataAccess.Interfaces
{
    public interface IChunkedWriter
    {
        public event Action<Chunk, bool> ChunkWritten;
        public void WriteChunk(Chunk chunk, bool isLastChunk);
    }
}
