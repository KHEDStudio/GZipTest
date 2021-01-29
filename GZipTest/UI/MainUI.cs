using GZipTest.ChunkedHandlers.ChunkedGZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.UI
{
    public class MainUI : IDisposable
    {
        private ThreadManager _threadManager;
        private ProgressBarAdapter _progressBar;
        private readonly int MaxBytesChunk = 1024 * 1024;

        public int Run(string[] args)
        {
            ShowInfo();
            var (compressionMode, sourceFilePath, destinationFilePath) = GetValidatedParams(args);
            var sourceFile = new FileInfo(sourceFilePath);
            var destinationFile = new FileInfo(destinationFilePath);
            _progressBar = new ProgressBarAdapter();
            if (compressionMode == "compress")
            {
                var compressor = new ChunkedGZipCompressor();
                _threadManager = new ThreadManager(sourceFile, MaxBytesChunk, compressor, destinationFile);
                _progressBar.SetStatus("Compressing...");
            }
            else
            {
                var decompressor = new ChunkedGZipDecompressor();
                _threadManager = new ThreadManager(sourceFile, decompressor, destinationFile);
                _progressBar.SetStatus("Decompressing...");
            }
            _threadManager.UpdateInfoText += msg => WriteConsoleWithTag(msg);
            _threadManager.PercentProgressChanged += _progressBar.SetProgress;
            Console.CancelKeyPress += (s, e) =>
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    _progressBar?.SetStatus("Cancelling...");
                    e.Cancel = true;
                    _threadManager?.Cancel();
                }
            };
            return _threadManager.Run();
        }

        private (string, string, string) GetValidatedParams(string[] args)
        {
            var validator = new Validator();
            var (isValidated, message) = validator.TryValidate(args);
            while (isValidated == false)
            {
                WriteConsoleWithTag(message);
                var compressionMode = GetInputData("Write compression mode (compress/decompress): ");
                var sourceFilePath = GetInputData("Write source file path: ");
                var destinationFilePath = GetInputData("Write destination file path: ");
                args = new string[] { compressionMode, sourceFilePath, destinationFilePath };
                (isValidated, message) = validator.TryValidate(args);
            }
            return (args[0], args[1], args[2]);
        }

        private string GetInputData(string message)
        {
            WriteConsoleWithTag(message, false);
            return Console.ReadLine();
        }

        public void WriteConsole(string message)
        {
            if (_progressBar == null)
                Console.WriteLine(message);
            else _progressBar.WriteLine(message);
        }

        public void WriteConsoleWithTag(string message, bool newLine = true)
        {
            if (_progressBar == null)
            {
                if (newLine)
                    Console.WriteLine($"[{DateTime.Now}] {message}");
                else
                    Console.Write($"[{DateTime.Now}] {message}");
            }
            else _progressBar.WriteLine($"[{DateTime.Now}] {message}");
        }

        private void ShowInfo()
        {
            WriteConsole("To compress or decompress files please proceed with the following pattern to type in:\n" +
                              "Compressing: GZipTest.exe compress [Source file path] [Destination file path]\n" +
                              "Decompressing: GZipTest.exe decompress [Compressed file path] [Destination file path]\n" +
                              "To complete the program correct please use the combination CTRL + C");
        }

        public void Dispose()
        {
            if (_threadManager != null)
                _threadManager.Dispose();
            _progressBar.SetStatus(string.Empty);
            _progressBar.Dispose();
        }
    }
}
