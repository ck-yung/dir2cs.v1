using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dir2
{
    static class Helper
    {
        static public string[] emptyStrings = new string[] { };

        static public InfoSum Invoke(this IEnumerable<InfoFile> seqThe,
            IFunc<IEnumerable<InfoFile>, InfoSum> func)
        {
            return func.Func(seqThe);
        }

        static public string GetFirstPath(string arg)
        {
            var parts = arg.Split(Path.DirectorySeparatorChar);
            if (parts.Length == 1) return ".";
            return parts[0];
        }

        static IEnumerator<string> SafeGetFileEnumerator(string dirname)
        {
            try { return Directory.EnumerateFiles(dirname).GetEnumerator(); }
            catch { return emptyStrings.AsEnumerable().GetEnumerator(); }
        }

        static public IEnumerable<T> Invoke<T>(
            this IEnumerable<T> seqThe,
            Func<IEnumerable<T>, IEnumerable<T>> func)
        {
            return func(seqThe);
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

        static public IEnumerable<string> GetFiles(string dirname)
        {
            var enumFile = SafeGetFileEnumerator(dirname);
            while (SafeMoveNext(enumFile))
            {
                var currentFilename = SafeGetCurrent(enumFile);
                if (string.IsNullOrEmpty(currentFilename)) continue;
                yield return currentFilename;
            }
        }

        static public void PrintDir(string dirname)
        {
            var cntDir = 0;
            var enumDir = SafeGetDirectoryEnumerator(dirname);
            while (SafeMoveNext(enumDir))
            {
                var currentDirname = SafeGetCurrent(enumDir);
                if (string.IsNullOrEmpty(currentDirname)) continue;
                Console.Write(Opts.ItemText(
                    $"[DIR] {InfoFile.RelativePath(currentDirname)}"));
                cntDir += 1;
            }
            if (cntDir > 1)
            {
                Console.Write(Opts.TotalText(
                    $"{cntDir} directories are found."));
                Console.Write(Opts.TotalText(""));
            }
        }
    }

    class TooManyValuesException: ArgumentException
    {
        public TooManyValuesException(string message)
            : base(message) { }
    }

    class InvalidValueException : ArgumentException
    {
        public InvalidValueException(string value, string optName)
            : base($"'{value}' to {optName}") { }
    }
}
