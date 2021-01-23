using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.UI
{
    public class Validator
    {
        public (bool, string) TryValidate(string[] args)
        {
            if (args.Length == 0 || args.Length > 3)
            {
                return (false, "Please enter arguments: [compress(decompress)] [Source file] [Destination file].");
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                return (false, "First argument shall be \"compress\" or \"decompress\".");
            }

            if (args[1].Length == 0)
            {
                return (false, "No source file name was specified.");
            }

            if (File.Exists(args[1]) == false)
            {
                return (false, "No source file was found.");
            }

            FileInfo _fileIn = new FileInfo(args[1]);
            FileInfo _fileOut = new FileInfo(args[2]);

            if (args[1] == args[2])
            {
                return (false, "Source and destination files shall be different.");
            }

            if (_fileIn.Extension == ".gz" && args[0] == "compress")
            {
                return (false, "File has already been compressed.");
            }

            if (_fileOut.Extension == ".gz" && _fileOut.Exists)
            {
                return (false, "Destination file already exists. Please indiciate the different file name.");
            }

            if (_fileIn.Extension != ".gz" && args[0] == "decompress")
            {
                return (false, "File to be decompressed shall have .gz extension.");
            }

            if (args[2].Length == 0)
            {
                return (false, "No destination file name was specified.");
            }

            return (true, string.Empty);
        }
    }
}
