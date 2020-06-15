using System.Collections.Generic;
using System.IO;

namespace dir2
{
    static class Helper
    {
        static public IEnumerable<string> GetAllFiles(string dirname)
        {
            var enumFile = Directory.EnumerateFiles(dirname).GetEnumerator();
            while (enumFile.MoveNext())
            {
                var currentFilename = enumFile.Current;
                yield return currentFilename;
            }

            var enumDir = Directory.EnumerateDirectories(dirname).GetEnumerator();
            while (enumDir.MoveNext())
            {
                var currentDirname = enumDir.Current;
                yield return $"[DIR] {currentDirname}";
            }
        }
    }
}
