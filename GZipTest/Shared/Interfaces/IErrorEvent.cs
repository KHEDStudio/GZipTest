using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.Shared.Interfaces
{
    public interface IErrorEvent
    {
        public event Action<ErrorEventArgs> OnError;
    }
}
