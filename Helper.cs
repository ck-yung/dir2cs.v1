using System;
using System.Collections.Generic;
using System.IO;

namespace dir2
{
    static class Helper
    {
        static public IEnumerable<string> GetAllFiles(string dirname)
        {
            return Directory.EnumerateFiles(dirname);
        }
    }
}
