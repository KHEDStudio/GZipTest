using GZipTest.ChunkedHandlers.Interfaces;
using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using GZipTest.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace GZipTest.ChunkedHandlers
{
    public abstract class ChunkedHandler : IChunkedHandler, IErrorEvent, IDisposable
    {
        public event Action<Chunk, bool> ChunkHandled;
        public event Action<ErrorEventArgs> OnError;

        private readonly int MaxQueueChunksSize = 3;
        private AutoResetEvent _queueTakeEvent;
        private BlockingCollection<Chunk> _chunks;

        public ChunkedHandler()
        {
            _queueTakeEvent = new AutoResetEvent(false);
            _chunks = new BlockingCollection<Chunk>();
        }

        public void AddChunkToQueue(Chunk chunk, bool isLastChunk)
        {
            while (_chunks.Count >= MaxQueueChunksSize)
                _queueTakeEvent.WaitOne();
            _chunks.Add(chunk);
            if (isLastChunk)
                _chunks.CompleteAdding();
        }

        public void Start()
        {
            try
            {
                while (_chunks.IsCompleted == false)
                {
                    bool isRemoved = _chunks.TryTake(out var chunk);
                    _queueTakeEvent.Set();
                    if (isRemoved)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            WriteChunkToStream(memoryStream, chunk);
                            var proccessedBytes = GetBytesFromStream(memoryStream);
                            ChunkHandled?.Invoke(new Chunk(chunk.Index, proccessedBytes), _chunks.IsCompleted);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_chunks.IsAddingCompleted == false)
                    OnError?.Invoke(new ErrorEventArgs(ex));
            }
        }

        private byte[] GetBytesFromStream(MemoryStream memoryStream)
        {
            var buffer = memoryStream.ToArray();
            return buffer;
        }

        protected abstract void WriteChunkToStream(MemoryStream memoryStream, Chunk chunk);

        public void Dispose()
        {
            _chunks.CompleteAdding();
        }
    }
}
