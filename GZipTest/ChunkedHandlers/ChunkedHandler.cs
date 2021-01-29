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
        private readonly object _syncTakeChunk;
        private AutoResetEvent _queueTakeEvent;
        private BlockingCollection<(Chunk, bool)> _chunks;

        public ChunkedHandler()
        {
            _syncTakeChunk = new object();
            _queueTakeEvent = new AutoResetEvent(false);
            _chunks = new BlockingCollection<(Chunk, bool)>();
        }

        public void AddChunkToQueue(Chunk chunk, bool isLastChunk)
        {
            while (_chunks.Count >= MaxQueueChunksSize)
                _queueTakeEvent.WaitOne();
            _chunks.Add((chunk, isLastChunk));
        }

        public void Start()
        {
            Chunk chu;
            try
            {
                while (_chunks.IsCompleted == false)
                {
                    (Chunk, bool) chunkPair;
                    bool isRemoved, isQueueCompleted;
                    lock (_syncTakeChunk)
                    {
                        isRemoved = _chunks.TryTake(out chunkPair);
                        isQueueCompleted = _chunks.IsCompleted;
                    }
                    if (isRemoved)
                    {
                        chu = chunkPair.Item1;
                        _queueTakeEvent.Set();
                        using (var memoryStream = new MemoryStream())
                        {
                            var (chunk, isLastChunk) = chunkPair;
                            WriteChunkToStream(memoryStream, chunk);
                            var proccessedBytes = GetBytesFromStream(memoryStream);
                            ChunkHandled?.Invoke(new Chunk(chunk.Index, proccessedBytes),
                                isLastChunk || isQueueCompleted);
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
            if (_chunks.IsAddingCompleted == false)
            {
                _chunks.CompleteAdding();
                _queueTakeEvent.Close();
            }
        }
    }
}
