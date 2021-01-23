using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZipTest.UI
{
    public class ProgressBarAdapter : IDisposable
    {
        private ProgressBar _progressBar;
        private IProgress<double> _progress;
        private readonly int MaxTicksBar = 10000;

        public ProgressBarAdapter()
        {
            _progressBar = new ProgressBar(MaxTicksBar, "Starting...");
            _progress = _progressBar.AsProgress<double>();
        }

        public void SetProgress(double percent) => _progress.Report(percent / 100);

        public void WriteLine(string message) => _progressBar.WriteLine(message);

        public void SetStatus(string status) => _progressBar.Message = status;

        public void Dispose()
        {
            _progressBar.Dispose();
        }
    }
}
