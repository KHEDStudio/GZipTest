using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest.ChunkedHandlers.ChunkedGZip
{
    public class ChunkedGZipDecompressor : ChunkedHandler
    {
        protected override void WriteChunkToStream(MemoryStream memoryStream, Chunk chunk)
        {
            using (var chunkStream = new MemoryStream(chunk.Bytes))
            {
                using (var zipStream = new GZipStream(chunkStream, CompressionMode.Decompress))
                {
                    zipStream.CopyTo(memoryStream);
                }
            }
        }
    }
}
