using GZipTest.ChunkedHandlers;
using GZipTest.ChunkedHandlers.ChunkedGZip;
using GZipTest.DataAccess;
using GZipTest.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GZipTest
{
    public class ThreadManager : IDisposable
    {
        public event Action<string> UpdateInfoText;
        public event Action<double> PercentProgressChanged;

        private int _chunkIndex = 0;

        private ChunkedFileReader _reader;
        private ChunkedHandler _chunkedHandler;
        private ChunkedFileWriter _writer;

        private AutoResetEvent _allBytesWritten = new AutoResetEvent(false);
        private ManualResetEvent _chunkWriteEvent = new ManualResetEvent(false);
        private CancellationTokenSource _token = new CancellationTokenSource();

        private readonly int ThreadsNumber = Environment.ProcessorCount;

        public ThreadManager(FileInfo source, ChunkedGZipDecompressor chunkedHandler, FileInfo destination)
        {
            _reader = new ChunkedFileReader(source, _token.Token);
            InitializeModules(chunkedHandler, destination);
        }

        public ThreadManager(FileInfo source, int maxBytesChunk, ChunkedGZipCompressor chunkedHandler, FileInfo destination)
        {
            _reader = new ChunkedFileReader(source, maxBytesChunk, _token.Token);
            InitializeModules(chunkedHandler, destination);
        }

        private void InitializeModules(ChunkedHandler chunkedHandler, FileInfo destination)
        {
            _chunkedHandler = chunkedHandler ?? throw new ArgumentNullException(nameof(chunkedHandler));
            _writer = new ChunkedFileWriter(destination, _token.Token);
            _reader.ChunkRead += (chunk, isLastChunk) => chunkedHandler.AddChunkToQueue(chunk, isLastChunk);
            _reader.OnError += error => OnError($"While reading: {error.GetException().Message}");
            _chunkedHandler.ChunkHandled += (chunk, isLastChunk) =>
            {
                while (Interlocked.CompareExchange(ref _chunkIndex, 0, 0) != chunk.Index
                    && _token.IsCancellationRequested == false)
                    _chunkWriteEvent.WaitOne();
                _chunkWriteEvent.Reset();
                _writer.WriteChunk(chunk, isLastChunk);
                Interlocked.Increment(ref _chunkIndex);
                _chunkWriteEvent.Set();
                if (isLastChunk)
                    _allBytesWritten.Set();
            };
            _chunkedHandler.OnError += error => OnError($"While handling: {error.GetException().Message}");
            int decimalPlaces = 2;
            _writer.ChunkWritten += (chunk, isLastChunk) =>
            {
                double percent = Math.Round((double)_reader.TotalBytesRead / _reader.TotalBytes * 100, decimalPlaces);
                PercentProgressChanged?.Invoke(percent);
            };
            _writer.OnError += error => OnError($"While writting: {error.GetException().Message}");
        }

        public int Run()
        {
            for (int i = 0; i < ThreadsNumber;)
            {
                var thread = new Thread(_chunkedHandler.Start);
                thread.Name = $"Chunked handler: {++i}";
                thread.Start();
            }
            _reader.ReadAllBytes();
            _reader.Dispose();
            _allBytesWritten.WaitOne();
            return Convert.ToInt32(_token.IsCancellationRequested);
        }

        public void Cancel()
        {
            _token.Cancel();
            _chunkWriteEvent.Set();
            _allBytesWritten.Set();
            _chunkedHandler.Dispose();
        }

        private void OnError(string msg)
        {
            if (_token.IsCancellationRequested == false)
            {
                UpdateInfoText?.Invoke(msg);
                Cancel();
            }
        }

        public void Dispose()
        {
            _chunkedHandler.Dispose();
            _writer.Dispose();
        }
    }
}
