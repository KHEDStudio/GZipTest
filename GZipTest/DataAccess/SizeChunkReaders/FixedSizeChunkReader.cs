using GZipTest.DataAccess.Interfaces;
using GZipTest.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.DataAccess.SizeChunkReaders
{
    public class FixedSizeChunkReader : ISizeChunkReader
    {
        private int _maxBytesChunk;
        private Stream _stream;

        public FixedSizeChunkReader(int maxBytesChunk, Stream stream)
        {
            _maxBytesChunk = maxBytesChunk;
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public byte[] GetChunkBytes(int offset)
        {
            var reader = new BytesReader(_stream);
            return reader.GetBytes(offset, _maxBytesChunk);
        }
    }
}
