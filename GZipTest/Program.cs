using GZipTest.UI;
using ShellProgressBar;
using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var mainUI = new MainUI();
            return mainUI.Run(args);
        }
    }
}
