using GZipTest.ChunkedHandlers;
using GZipTest.ChunkedHandlers.ChunkedGZip;
using GZipTest.DataAccess;
using GZipTest.Test.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest.Test
{
    public class ChunkedGZipCompressor
    {
        private FileInfo _sourceFile;
        private FileInfo _compressedFile;

        private int _maxChunkSize = 1024 * 1024;

        private ChunkedFileWriter _compressorWriter;

        [SetUp]
        public void Setup()
        {
            _sourceFile = new FileInfo("makkonnell_c_sovershennyi_kod_master_klass.pdf");
            _compressedFile = new FileInfo(Guid.NewGuid().ToString());
            _compressorWriter = new ChunkedFileWriter(_compressedFile, new CancellationToken());
        }

        [Test]
        public void FileCompress()
        {
            var compressor = GetGZipCompressor();
            using (var reader = GetFileReader(compressor))
            {
                Task.Factory.StartNew(reader.ReadAllBytes, TaskCreationOptions.LongRunning);
                compressor.Start();
            }
            _compressorWriter.Dispose();
            Assert.Pass();
        }

        private ChunkedHandlers.ChunkedGZip.ChunkedGZipCompressor GetGZipCompressor()
        {
            var compressor = new ChunkedHandlers.ChunkedGZip.ChunkedGZipCompressor();
            compressor.ChunkHandled += (chunk, isLastChunk) => _compressorWriter.WriteChunk(chunk, isLastChunk);
            compressor.OnError += error => Assert.Fail(error.GetException().ToString());
            return compressor;
        }

        private ChunkedFileReader GetFileReader(ChunkedHandlers.ChunkedGZip.ChunkedGZipCompressor compressor)
        {
            var reader = new ChunkedFileReader(_sourceFile, _maxChunkSize, new CancellationToken());
            reader.ChunkRead += (chunk, isLastChunk) => compressor.AddChunkToQueue(chunk, isLastChunk);
            reader.OnError += error => Assert.Fail(error.GetException().ToString());
            return reader;
        }

        [TearDown]
        public void DeleteFile()
        {
            _compressedFile.Delete();
        }
    }
}
