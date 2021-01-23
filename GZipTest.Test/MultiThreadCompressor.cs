using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.Test
{
    public class MultiThreadCompressor
    {
        private FileInfo _source;
        private FileInfo _destination;
        private ThreadManager _manager;

        [SetUp]
        public void Setup()
        {
            _source = new FileInfo("makkonnell_c_sovershennyi_kod_master_klass.pdf");
            _destination = new FileInfo("makkonnell_c_sovershennyi_kod_master_klass_compressed.pdf.gz");
            _manager = new ThreadManager(_source, 1024 * 1024, new ChunkedHandlers.ChunkedGZip.ChunkedGZipCompressor(), _destination);
            _manager.UpdateInfoText += msg => Assert.Fail(msg);
            _manager.PercentProgressChanged += percent => Console.WriteLine(percent);
        }

        [Test]
        public void RunCompress()
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
