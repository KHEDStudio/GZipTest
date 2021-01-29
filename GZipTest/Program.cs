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
            using var mainUI = new MainUI();
            try
            {
                return mainUI.Run(args);
            }
            catch (Exception ex)
            {
                mainUI.WriteConsoleWithTag(ex.Message);
                return 1;
            }
        }
    }
}
