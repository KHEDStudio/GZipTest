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
        private BytesReader _reader;
        private readonly int GZipHeaderSize = 10; // RFC 1952
        private readonly byte[] GZipHeaderSignature;
        private readonly byte[] GZipMagicNumber = new byte[] { 0x1f, 0x8b };
        private readonly byte[] DeflateMethod = new byte[] { 0x08 };
        private readonly byte[] ReservedBitsFlags = new byte[] { 0xe0 };

        public UnknownSizeGZipChunkReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _reader = new BytesReader(_stream);
            GZipHeaderSignature = GetFirstHeaderSignature();
        }

        private byte[] GetFirstHeaderSignature()
        {
            int offset = 0, startPosition = 0;
            var currentPosition = _stream.Position;
            _stream.Position = startPosition;
            var buffer = GetGZipHeader(offset);
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
            var headerBytes = GetGZipHeader(offset);
            var listBytes = new List<byte>(headerBytes);
            var readByte = _stream.ReadByte();
            var headerMathsCount = 0;
            var headerSignatureOffset = GZipHeaderSignature.Length;
            while (readByte != -1)
            {
                listBytes.Add((byte)readByte);
                if (readByte == headerBytes[headerMathsCount])
                {
                    headerMathsCount++;
                    if (headerMathsCount == headerBytes.Length)
                    {
                        _stream.Position -= headerSignatureOffset;
                        return listBytes.ToArray();
                    }
                }
                else headerMathsCount = 0;
                readByte = _stream.ReadByte();
            }
            return listBytes.ToArray();
        }

        private byte[] GetGZipHeader(int offset) => _reader.GetBytes(offset, GZipHeaderSize);
    }
}
