using GZipTest.DataAccess.Interfaces;
using GZipTest.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.DataAccess.SizeChunkReaders
{
    public class UnknownSizeGZipChunkReader : ISizeChunkReader
    {
        private Stream _stream;
        private readonly int GZipHeaderSize = 10; // RFC 1952
        private readonly byte[] GZipHeaderSignature;
        private readonly byte[] GZipMagicNumber = new byte[] { 0x1f, 0x8b };
        private readonly byte[] DeflateMethod = new byte[] { 0x08 };
        private readonly byte[] ReservedBitsFlags = new byte[] { 0xe0 };

        public UnknownSizeGZipChunkReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            GZipHeaderSignature = GetFirstHeaderSignature();
        }

        private byte[] GetFirstHeaderSignature()
        {
            int offset = 0, startPosition = 0;
            var currentPosition = _stream.Position;
            _stream.Position = startPosition;
            var buffer = GetGZipHeader(offset).ToArray();
            _stream.Position = currentPosition;
            if (IsValidGZipHeader(buffer) == false)
                throw new Exception("The archive entry was compressed using an unsupported compression method.");
            return buffer;
        }

        private bool IsValidGZipHeader(byte[] header)
        {
            if (header[0] != GZipMagicNumber[0] || header[1] != GZipMagicNumber[1])
                return false;
            if (header[2] != DeflateMethod[0])
                return false;
            if (Convert.ToBoolean(header[3] & ReservedBitsFlags[0]))
                return false;
            return true;
        }

        public byte[] GetChunkBytes(int offset)
        {
            var listBytes = new List<byte>(GetGZipHeader(offset));
            var reader = new BytesReader(_stream);
            int _offset = 0, length = 1024 * 1024;
            var readBytes = reader.GetBytes(_offset, length);
            var headerSignatureOffset = GZipHeaderSignature.Length;
            var headerSignatureIndexOffset = GZipHeaderSignature.Length - 1;
            while (readBytes.Length != 0)
            {
                var startIndex = listBytes.Count;
                listBytes.AddRange(readBytes);
                for (int i = startIndex; i < listBytes.Count; i++)
                {
                    if (IsFoundGZipHeaderSignature(listBytes.GetRange(i - headerSignatureIndexOffset, headerSignatureOffset).ToArray()))
                    {
                        int offsetPosition = listBytes.Count - i + headerSignatureIndexOffset;
                        _stream.Position -= offsetPosition;
                        return listBytes.GetRange(0, i - headerSignatureIndexOffset).ToArray();
                    }
                }
                readBytes = reader.GetBytes(_offset, length);
            }
            return listBytes.ToArray();
        }

        private IEnumerable<byte> GetGZipHeader(int offset)
        {
            var reader = new BytesReader(_stream);
            return reader.GetBytes(offset, GZipHeaderSize);
        }

        private bool IsFoundGZipHeaderSignature(byte[] lastBytes)
        {
            for (int i = 0; i < GZipHeaderSignature.Length; i++)
            {
                if (GZipHeaderSignature[i] != lastBytes[i])
                    return false;
            }
            return true;
        }
    }
}
