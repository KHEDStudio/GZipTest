using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZipTest.ChunkedHandlers.Interfaces
{
    public interface IChunkedHandler
    {
        public event Action<Chunk, bool> ChunkHandled;
        public void AddChunkToQueue(Chunk chunk, bool isLastChunk);
    }
}
