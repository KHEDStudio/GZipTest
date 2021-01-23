using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Utils
{
    public class BytesReader
    {
        private Stream _stream;
        public long TotalReadBytes { get; private set; }

        public BytesReader(Stream stream)
        {
            TotalReadBytes = 0;
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public byte[] GetBytes(int offset)
        {
            var buffer = new byte[_stream.Length];
            var readBytes = _stream.Read(buffer, offset, buffer.Length);
            TotalReadBytes += readBytes;
            if (readBytes < buffer.Length)
                buffer = buffer.Take(readBytes).ToArray();
            return buffer;
        }

        public byte[] GetBytes(int offset, int length)
        {
            var buffer = new byte[length];
            var readBytes = _stream.Read(buffer, offset, buffer.Length);
            TotalReadBytes += readBytes;
            if (readBytes < buffer.Length)
                buffer = buffer.Take(readBytes).ToArray();
            return buffer;
        }
    }
}
