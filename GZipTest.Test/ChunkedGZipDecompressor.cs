using GZipTest.ChunkedHandlers.ChunkedGZip;
using GZipTest.DataAccess;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Test
{
    public class ChunkedGZipDecompressor
    {
        private FileInfo _compressedFile;
        private FileInfo _decompressedFile;

        private ChunkedFileWriter _decompressorWriter;

        [SetUp]
        public void Setup()
        {
            _compressedFile = new FileInfo("makkonnell_c_sovershennyi_kod_master_klass.pdf.gz");
            _decompressedFile = new FileInfo(Guid.NewGuid().ToString());
            _decompressorWriter = new ChunkedFileWriter(_decompressedFile, new System.Threading.CancellationToken());
        }

        [Test]
        public void FileDecompress()
        {
            var decompressor = GetGZipDecompressor();
            using (var reader = GetFileReader(decompressor))
            {
                Task.Factory.StartNew(reader.ReadAllBytes, TaskCreationOptions.LongRunning);
                decompressor.Start();
            }
            _decompressorWriter.Dispose();
            Assert.Pass();
        }

        private ChunkedHandlers.ChunkedGZip.ChunkedGZipDecompressor GetGZipDecompressor()
        {
            var decompressor = new ChunkedHandlers.ChunkedGZip.ChunkedGZipDecompressor();
            decompressor.ChunkHandled += (chunk, isLastChunk) => _decompressorWriter.WriteChunk(chunk, isLastChunk);
            decompressor.OnError += error => Assert.Fail(error.GetException().ToString());
            return decompressor;
        }

        private ChunkedFileReader GetFileReader(ChunkedHandlers.ChunkedGZip.ChunkedGZipDecompressor decompressor)
        {
            var reader = new ChunkedFileReader(_compressedFile, new System.Threading.CancellationToken());
            reader.ChunkRead += (chunk, isLastChunk) => decompressor.AddChunkToQueue(chunk, isLastChunk);
            reader.OnError += error => Assert.Fail(error.GetException().ToString());
            return reader;
        }

        [TearDown]
        public void DeleteFile()
        {
            _decompressedFile.Delete();
        }
    }
}
