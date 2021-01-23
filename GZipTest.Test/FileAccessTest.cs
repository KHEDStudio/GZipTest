using GZipTest.DataAccess;
using GZipTest.DataAccess.Interfaces;
using GZipTest.Shared.Models;
using GZipTest.Test.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.Test
{
    public class FileAccessTest
    {
        private FileInfo _writeFileInfo;
        private string _writeFileName;

        private FileInfo _readFileInfo;

        private long _proccessedWriteBytes = 0;
        private long _proccessedReadBytes = 0;
        private int _chunksCount = 1024;
        private int _maxChunkSize = 1024 * 1024;

        [SetUp]
        public void Setup()
        {
            _writeFileName = Guid.NewGuid().ToString();
            _writeFileInfo = new FileInfo(_writeFileName);
        }

        [Test]
        public void FileWrite()
        {
            using (var writer = new ChunkedFileWriter(_writeFileInfo, new System.Threading.CancellationToken()))
            {
                writer.ChunkWritten += (chunk, isLastChunk) => _proccessedWriteBytes += chunk.Bytes.Length;
                writer.OnError += error => Assert.Fail(error.GetException().StackTrace);
                var generator = new FileGenerator(_maxChunkSize);
                for (int i = 0; i < _chunksCount; i++)
                    writer.WriteChunk(generator.GenerateChunk(i), i + 1 == _chunksCount);
            }
            Assert.AreEqual(_proccessedWriteBytes, _writeFileInfo.Length);
        }

        [Test]
        public void FileRead()
        {
            _readFileInfo = GenerateFile();
            using (var reader = new ChunkedFileReader(_readFileInfo, _maxChunkSize, new System.Threading.CancellationToken()))
            {
                reader.ChunkRead += (chunk, isLastChunk) => _proccessedReadBytes += chunk.Bytes.Length;
                reader.OnError += error => Assert.Fail(error.GetException().StackTrace);
                reader.ReadAllBytes();
            }
            Assert.AreEqual(_proccessedReadBytes, _readFileInfo.Length);
        }

        private FileInfo GenerateFile()
        {
            var generator = new FileGenerator(_maxChunkSize);
            return generator.GenerateFile(_chunksCount);
        }

        [TearDown]
        public void DeleteFiles()
        {
            _writeFileInfo.Delete();
            if (_readFileInfo != null)
                _readFileInfo.Delete();
        }
    }
}
