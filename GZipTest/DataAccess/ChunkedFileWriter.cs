using GZipTest.DataAccess.Interfaces;
using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GZipTest.DataAccess
{
    public class ChunkedFileWriter : IChunkedWriter, IErrorEvent, IDisposable
    {
        public event Action<Chunk, bool> ChunkWritten;
        public event Action<ErrorEventArgs> OnError;

        private FileInfo _fileInfo;
        private FileStream _fileStream;
        private CancellationToken _token;

        public ChunkedFileWriter(FileInfo fileInfo, CancellationToken token)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _fileStream = new FileStream(_fileInfo.FullName, FileMode.OpenOrCreate);
            _token = token;
        }

        public void WriteChunk(Chunk chunk, bool isLastChunk)
        {
            try
            {
                if (_token.IsCancellationRequested == false)
                {
                    int offset = 0;
                    _fileStream.Write(chunk.Bytes, offset, chunk.Bytes.Length);
                    _fileStream.Flush();
                    ChunkWritten?.Invoke(chunk, isLastChunk);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(ex));
            }
        }

        public void Dispose()
        {
            _fileStream.Flush();
            _fileStream.Close();
            if (_token.IsCancellationRequested)
                _fileInfo.Delete();
        }
    }
}
