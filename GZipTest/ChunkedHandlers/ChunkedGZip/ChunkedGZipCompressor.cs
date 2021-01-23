using GZipTest.ChunkedHandlers.Interfaces;
using GZipTest.Shared.Interfaces;
using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest.ChunkedHandlers.ChunkedGZip
{
    public class ChunkedGZipCompressor : ChunkedHandler
    {
        protected override void WriteChunkToStream(MemoryStream memoryStream, Chunk chunk)
        {
            using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                int offset = 0;
                zipStream.Write(chunk.Bytes, offset, chunk.Bytes.Length);
            }
        }
    }
}
