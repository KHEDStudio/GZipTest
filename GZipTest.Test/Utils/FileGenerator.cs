using GZipTest.DataAccess;
using GZipTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.Test.Utils
{
    public class FileGenerator
    {
        private int _maxChunkSize;

        public FileGenerator(int maxChunkSize)
        {
            _maxChunkSize = maxChunkSize > 0 ? maxChunkSize : throw new ArgumentOutOfRangeException(nameof(maxChunkSize));
        }

        public FileInfo GenerateFile(int chunksCount)
        {
            var fileName = Guid.NewGuid().ToString();
            var fileInfo = new FileInfo(fileName);
            WriteRandomBytes(fileInfo, chunksCount);
            return fileInfo;
        }

        private void WriteRandomBytes(FileInfo fileInfo, int chunksCount)
        {
            using (var writer = new ChunkedFileWriter(fileInfo, new System.Threading.CancellationToken()))
            {
                for (int i = 0; i < chunksCount; i++)
                {
                    var chunk = GenerateChunk(i);
                    writer.WriteChunk(chunk, i + 1 == chunksCount);
                }
            }
        }

        public Chunk GenerateChunk(int index)
        {
            var rnd = new Random();
            var bytes = new byte[_maxChunkSize];
            rnd.NextBytes(bytes);
            return new Chunk(index, bytes);
        }
    }
}
