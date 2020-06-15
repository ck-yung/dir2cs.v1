using System.Collections.Generic;
using System.IO;

namespace dir2
{
    static class Helper
    {
        static public IEnumerable<string> GetAllFiles(string dirname)
        {
            foreach (var filename in Directory.EnumerateFiles(dirname))
                yield return filename;
        }
    }
}
