using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dir2
{
    static class Helper
    {
        static public string[] emptyStrings = Array.Empty<string>();

        static public readonly ImmutableDictionary<string, string>
            ShortCutWithValue = new Dictionary<string, string>()
            {
                ["-o"] = "--sort=",
                ["-x"] = "--excl-file=",
                ["-X"] = "--excl-dir=",
                ["-n"] = "--name=",
            }.ToImmutableDictionary();

        static public readonly ImmutableDictionary<string, string[]>
            ShortCutWithoutValue = new Dictionary<string, string[]>()
            {
                ["-s"] = new string[] { "--dir=sub" },
                ["-f"] = new string[] { "--dir=off" },
                ["-d"] = new string[] { "--dir=only" },
                ["-T"] = new string[] { "--dir=tree" },
                ["-b"] = new string[] { "--total=off", "--hide=size,date,count" },
                ["-c"] = new string[] { "--case-sensitive" },
                ["-r"] = new string[] { "--reverse" },
                ["-t"] = new string[] { "--total=only" },
            }.ToImmutableDictionary();

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
                    else foreach (var chOpt in curr2[1..])
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

        static public void PrintDir(string dirname, Func<string, bool> filterThe)
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
                Console.Write(Opts.ItemText(Opts.DirNameText(
                    InfoFile.RelativePath(currentDirname))));
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
            static long unitValue(char unitThe)
            {
                return unitThe switch
                {
                    'k' => 1024,
                    'm' => 1024 * 1024,
                    _ => 1024 * 1024 * 1024,// g
                };
            }

            if (Regex.Match(arg, @"^\d+[kmg]$").Success)
            {
                if (long.TryParse(arg.AsSpan(0, arg.Length - 1),
                    out long valThe))
                {
                    if (valThe > 0)
                    {
                        result = valThe * unitValue(arg[^1]);
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

        static public readonly ImmutableArray<string> DateTimeFormats =
            ImmutableArray.Create(new String[] {
                "yyyy-MM-dd", "yyyyMMdd", "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm",
                "yyyy-MM-dd HH:mm", "yyyyMMdd HH:mm:ss",
                "yyyyMMdd HH:mm",
            });

        static public bool TryParseDateTime(string arg, out DateTime result)
        {
            result = DateTime.MinValue;

            if (Regex.Match(arg, @"^\d+[mhd]$").Success)
            {
                Func<int, TimeSpan> toTimeSpan = arg[^1] switch
                {
                    'm' => (arg9) => TimeSpan.FromMinutes(arg9),
                    'h' => (arg9) => TimeSpan.FromHours(arg9),
                    // 'd'
                    _ => (arg9) => TimeSpan.FromDays(arg9),
                };
                if (int.TryParse(arg.AsSpan(0, arg.Length - 1),
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
            var regThe = Opts.MakeRegex(
                Opts.ToRegexText.Func(arg));
            return (it) => regThe.Match(it).Success;
        }

        static public void PrintSubTree(string prefix, string dirname)
        {
            var enumDir = SafeGetDirectoryEnumerator(dirname);

            string GetNext()
            {
                while (SafeMoveNext(enumDir))
                {
                    var currDir = SafeGetCurrent(enumDir);
                    var dirThe = Path.GetFileName(currDir);
                    if (!Opts.ExclDirnameFilter.Func(dirThe))
                    {
                        return currDir;
                    }
                }
                return string.Empty;
            }

            var prevDir = GetNext();

            while (true)
            {
                var currDir = GetNext();
                if (string.IsNullOrEmpty(currDir)) break;
                var dirThe = Path.GetFileName(prevDir);
                Console.WriteLine($"{prefix}+- {dirThe}");
                PrintSubTree($"{prefix}|  ", prevDir);
                prevDir = currDir;
            }

            if (!string.IsNullOrEmpty(prevDir))
            {
                var dirThe = Path.GetFileName(prevDir);
                Console.WriteLine($"{prefix}\\- {dirThe}");
                PrintSubTree($"{prefix}   ", prevDir);
            }
        }

        static public void PrintTree(string dirname)
        {
            Console.WriteLine(dirname);
            PrintSubTree("", dirname);
        }

        static public IEnumerable<string> EnvirExpandShortcut(
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
                    else
                    {
                        int ii = 1; var cc = curr2[ii];
                        for (; ii < curr2.Length; ii++)
                        {
                            cc = curr2[ii];
                            if (cc==' ') break;
                            yield return $"-{cc}";
                        }
                        if (cc==' ')
                        {
                            if (ii<(curr2.Length-1))
                            {
                                yield return curr2[ii..].TrimStart();
                            }
                            else
                            {
                                throw new Exception(
                                    $"Bad envir opt '{curr2}'");
                            }
                        }
                    }
                }
            }

            IEnumerable<string> ExpandShortcut()
            {
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

            try
            {
                return ExpandShortcut().ToArray();
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine($"Envir error: {ee.Message}");
                return Helper.emptyStrings;
            }
        }

        static public IEnumerable<string> EnvirParse(
            IEnumerable<string> args)
        {
            try
            {
                return Opts.EnvirParsers
                    .Aggregate(args, (it, opt) => opt.Parse(it))
                    .Join(Opts.ExclFileDirParsers,
                    outerKeySelector: (line) => line.Split('=')[0],
                    innerKeySelector: (opt) => opt.Name().Trim('='),
                    resultSelector: (line, opt) => line)
                    .ToArray();
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine($"Envir error: {ee.Message}");
                return Helper.emptyStrings;
            }
        }

        static public class Print
        {
            static public Func<string,string>
            Off = (it) => String.Empty;
            static public Func<string,string>
            Item = (it) => it;
            static public Func<string,string>
            Line = (it) => $"{it}{Environment.NewLine}";
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
