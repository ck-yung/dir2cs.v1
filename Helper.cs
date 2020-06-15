using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dir2
{
    static class Helper
    {
        static public string[] emptyStrings = new string[] { };

        static IEnumerator<string> SafeGetFileEnumerator(string dirname)
        {
            try { return Directory.EnumerateFiles(dirname).GetEnumerator(); }
            catch { return emptyStrings.AsEnumerable().GetEnumerator(); }
        }

        static IEnumerator<string> SafeGetDirectoryEnumerator(string dirname)
        {
            try { return Directory.EnumerateDirectories(dirname).GetEnumerator(); }
            catch { return emptyStrings.AsEnumerable().GetEnumerator(); }
        }

        static bool SafeMoveNext(IEnumerator<string> it)
        {
            try { return it.MoveNext(); }
            catch { return false; }
        }

        static string SafeGetCurrent(IEnumerator<string> it)
        {
            try { return it.Current; }
            catch { return string.Empty; }
        }

        static public IEnumerable<string> GetAllFiles(string dirname)
        {
            var enumFile = SafeGetFileEnumerator(dirname);
            while (SafeMoveNext(enumFile))
            {
                var currentFilename = SafeGetCurrent(enumFile);
                if (string.IsNullOrEmpty(currentFilename)) continue;
                yield return currentFilename;
            }

            var enumDir = SafeGetDirectoryEnumerator(dirname);
            while (enumDir.MoveNext())
            {
                var currentDirname = SafeGetCurrent(enumDir);
                if (string.IsNullOrEmpty(currentDirname)) continue;
                if (currentDirname.EndsWith(".git")) continue;
                foreach (var filename in GetAllFiles(currentDirname))
                {
                    yield return filename;
                }
            }
        }
    }
}
