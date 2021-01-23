using GZipTest.DataAccess.Interfaces;
using GZipTest.DataAccess.SizeChunkReaders;
using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using GZipTest.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GZipTest.DataAccess
{
    public class ChunkedFileReader : IChunkedReader, IErrorEvent, IDisposable
    {
        public event Action<Chunk, bool> ChunkRead;
        public event Action<ErrorEventArgs> OnError;

        private FileInfo _fileInfo;
        private FileStream _fileStream;
        private CancellationToken _cancellationToken;

        private ISizeChunkReader _reader;

        public long TotalBytes { get; private set; }
        public long TotalBytesRead { get; private set; }

        public ChunkedFileReader(FileInfo fileInfo, CancellationToken token)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _fileStream = new FileStream(_fileInfo.FullName, FileMode.Open);
            _reader = new UnknownSizeGZipChunkReader(_fileStream);
            TotalBytes = _fileStream.Length;
            TotalBytesRead = _fileStream.Position;
            _cancellationToken = token;
        }

        public ChunkedFileReader(FileInfo fileInfo, int maxBytesChunk, CancellationToken token)
        {
            if (maxBytesChunk <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBytesChunk));
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _fileStream = new FileStream(_fileInfo.FullName, FileMode.Open);
            _reader = new FixedSizeChunkReader(maxBytesChunk, _fileStream);
            TotalBytes = _fileStream.Length;
            TotalBytesRead = _fileStream.Position;
            _cancellationToken = token;
        }

        public long ReadAllBytes()
        {
            try
            {
                byte[] buffer;
                int offset = 0, chunkIndex = 0;
                do
                {
                    if (_cancellationToken.IsCancellationRequested)
                        return TotalBytesRead;
                    buffer = _reader.GetChunkBytes(offset);
                    TotalBytesRead += buffer.Length;
                    if (buffer.Length > 0)
                        ChunkRead?.Invoke(new Chunk(chunkIndex++, buffer), _fileStream.Length == _fileStream.Position);
                } while (buffer.Length != 0);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(ex));
            }
            return TotalBytesRead;
        }

        public void Dispose()
        {
            _fileStream.Flush();
            _fileStream.Close();
        }
    }
}
