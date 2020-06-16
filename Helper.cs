using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dir2
{
    static class Helper
    {
        static public string[] emptyStrings = new string[] { };

        static public readonly Dictionary<string, string>
            ShortCutWithValue =
            new Dictionary<string, string>()
            {
                ["-o"] = "--sort=",
                ["-x"] = "--excl-file=",
                ["-X"] = "--excl-dir=",
                ["-n"] = "--name=",
            };

        static public readonly Dictionary<string, string[]>
            ShortCutWithoutValue =
            new Dictionary<string, string[]>()
            {
                ["-s"] = new string[] { "--dir=sub" },
                ["-f"] = new string[] { "--dir=off" },
                ["-d"] = new string[] { "--dir=only" },
                ["-c"] = new string[] { "--case-sensitive" },
                ["-b"] = new string[] { "--total=off", "--hide=size,date,count" },
                ["-t"] = new string[] { "--total=only" },
            };

        static public IEnumerable<string> ExpandShortcut(
            this IEnumerable<string> args)
        {
            IEnumerable<string> ExpandCombiningShortcut()
            {
                var enum2 = args.AsEnumerable().GetEnumerator();
                while (enum2.MoveNext())
                {
                    var curr2 = enum2.Current;
                    if (curr2.Length < 3) yield return curr2;
                    else if (curr2.StartsWith("--")) yield return curr2;
                    else if (curr2[0] != '-') yield return curr2;
                    else foreach (var chOpt in curr2.Substring(1))
                            yield return $"-{chOpt}";
                }
            }

            var enumThe = ExpandCombiningShortcut().GetEnumerator();
            while (enumThe.MoveNext())
            {
                var current = enumThe.Current;
                if (ShortCutWithValue.ContainsKey(current))
                {
                    if (!enumThe.MoveNext())
                    {
                        throw new ArgumentException(
                            $"Missing value to '{current}','{ShortCutWithValue[current]}'");
                    }
                    var valueThe = enumThe.Current;
                    yield return $"{ShortCutWithValue[current]}{valueThe}";
                }
                else if (ShortCutWithoutValue.ContainsKey(current))
                {
                    foreach (var valueThe in ShortCutWithoutValue[current])
                    {
                        yield return valueThe;
                    }
                }
                else
                {
                    yield return current;
                }
            }
        }

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
                if (Opts.ExclDirnameFilter.Func(
                    Path.GetFileName(currentDirname))) continue;
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

        static public void PrintDir(string dirname, Func<string,bool> filterThe)
        {
            var cntDir = 0;
            var enumDir = SafeGetDirectoryEnumerator(dirname);
            while (SafeMoveNext(enumDir))
            {
                var currentDirname = SafeGetCurrent(enumDir);
                if (string.IsNullOrEmpty(currentDirname)) continue;
                var nameThe = Path.GetFileName(currentDirname);
                if (Opts.ExclDirnameFilter.Func(nameThe)) continue;
                if (!filterThe(nameThe)) continue;
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

        public static bool TryParseAsLong(string arg, out long result)
        {
            long unitValue(char unitThe)
            {
                switch (unitThe)
                {
                    case 'k': return 1024;
                    case 'm': return 1024 * 1024;
                    default: return 1024 * 1024 * 1024; // g
                }
            }

            if (Regex.Match(arg, @"^\d+[kmg]$").Success)
            {
                if (long.TryParse(arg.Substring(0, arg.Length - 1),
                    out long valThe))
                {
                    if (valThe > 0)
                    {
                        result = valThe * unitValue(arg[arg.Length - 1]);
                        return true;
                    }
                }
            }
            else
            {
                if (long.TryParse(arg, out long valThe))
                {
                    if (valThe >= 0)
                    {
                        result = valThe;
                        return true;
                    }
                }
            }
            result = 0;
            return false;
        }

        static public string[] DateTimeFormats = new string[] {
                "yyyy-MM-dd", "yyyyMMdd", "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm",
                "yyyy-MM-dd HH:mm", "yyyyMMdd HH:mm:ss",
                "yyyyMMdd HH:mm",
            };

        static public bool TryParseDateTime(string arg, out DateTime result)
        {
            result = DateTime.MinValue;

            if (Regex.Match(arg, @"^\d+[mhd]$").Success)
            {
                Func<int, TimeSpan> toTimeSpan;
                switch (arg[arg.Length - 1])
                {
                    case 'm':
                        toTimeSpan = (arg9) => TimeSpan.FromMinutes(arg9);
                        break;
                    case 'h':
                        toTimeSpan = (arg9) => TimeSpan.FromHours(arg9);
                        break;
                    default: // 'd'
                        toTimeSpan = (arg9) => TimeSpan.FromDays(arg9);
                        break;
                }

                if (int.TryParse(arg.Substring(0, arg.Length - 1),
                    out int goodValue))
                {
                    if (goodValue > 0)
                    {
                        result = DateTime.Now.Subtract(toTimeSpan(goodValue));
                        return true;
                    }
                }
                return false;
            }

            foreach (var fmtThe in DateTimeFormats)
            {
                if (DateTime.TryParseExact(arg, fmtThe,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime goodValue))
                {
                    result = goodValue;
                    return true;
                }
            }

            return false;
        }

        static public string ToKiloUnit(long arg)
        {
            var units = new char[] { 'T', 'G', 'M', 'K', ' ' };
            string toKilo(long arg2, int index)
            {
                if (arg2 < 10000) return $"{arg2,4}{units[index - 1]}";
                if (index == 1) return $"{arg2,4}{units[0]}";
                return toKilo((arg2 + 512) / 1024, index - 1);
            }
            return toKilo(arg, units.Length);
        }

        static public Func<string, bool> ToWildMatch(string arg)
        {
            var regText = new StringBuilder("^");
            regText.Append(arg
                .Replace(@"\", @"\\")
                .Replace("^", @"\^")
                .Replace("$", @"\$")
                .Replace(".", @"\.")
                .Replace("?", ".")
                .Replace("*", ".*")
                .Replace("(", @"\(")
                .Replace(")", @"\)")
                .Replace("[", @"\[")
                .Replace("]", @"\]")
                .Replace("{", @"\{")
                .Replace("}", @"\}"));
            regText.Append("$");
            var regThe = Opts.MakeRegex(regText.ToString());
            return (it) => regThe.Match(it).Success;
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
