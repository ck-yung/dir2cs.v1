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
                var dirnameThe = Path.Join(dirname, Path.GetFileName(currentDirname));
                var enumFile2 = Directory.EnumerateFiles(dirnameThe).GetEnumerator();
                while (enumFile2.MoveNext())
                {
                    var currentFilename = enumFile2.Current;
                    yield return currentFilename;
                }
            }
        }
    }
}
