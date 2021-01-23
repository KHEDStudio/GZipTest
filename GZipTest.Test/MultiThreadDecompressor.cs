using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.Test
{
    public class MultiThreadDecompressor
    {
        private FileInfo _source;
        private FileInfo _destination;
        private ThreadManager _manager;

        [SetUp]
        public void Setup()
        {
            _source = new FileInfo("makkonnell_c_sovershennyi_kod_master_klass.pdf.gz");
            _destination = new FileInfo("makkonnell_c_sovershennyi_kod_master_klass_decompressed.pdf");
            _manager = new ThreadManager(_source, new ChunkedHandlers.ChunkedGZip.ChunkedGZipDecompressor(), _destination);
            _manager.UpdateInfoText += msg => Assert.Fail(msg);
            _manager.PercentProgressChanged += percent => Console.WriteLine(percent);
        }

        [Test]
        public void RunDecompress()
        {
            _manager.Run();
            _manager.Dispose();
            Assert.Pass();
        }

        [TearDown]
        public void DeleteFile()
        {
            _destination.Delete();
        }
    }
}
