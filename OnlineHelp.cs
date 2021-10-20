using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace dir2
{
    static class OnlineHelp
    {
        static public bool IsShow(string[] args)
        {
            var assemblyThe = Assembly.GetExecutingAssembly();

            string Get<T>(Func<T, string> select)
                where T : Attribute
            {
                if (Attribute.IsDefined(assemblyThe,typeof(T)))
                {
                    T attribute = (T)Attribute.GetCustomAttribute(
                        assemblyThe, typeof(T))!;
                    return select.Invoke(attribute);
                }
                else
                {
                    return string.Empty;
                }
            }

            if (args.Contains("-v") || args.Contains("--version"))
            {
                var title = Get<AssemblyTitleAttribute>(it =>
                it.Title);
                var version = Get<AssemblyFileVersionAttribute>(
                    it => it.Version);
                Console.WriteLine($"{title} Version {version}");
                var copyright = Get<AssemblyCopyrightAttribute>(
                    it => it.Copyright);
                Console.WriteLine(copyright);
                var company = Get<AssemblyCompanyAttribute>(
                    it => it.Company);
                Console.WriteLine(company);
                var description = Get<AssemblyDescriptionAttribute>(
                    it => it.Description);
                Console.WriteLine(description);
                return true;
            }

            if (args.Contains("--help"))
            {
                Console.WriteLine(
                    $"Syntax: dir2 DIR{Path.DirectorySeparatorChar}WILD [OPT ..]");
                Console.WriteLine("Syntax: dir2 [DIR] [WILD ..] [OPT ..]");
                Console.WriteLine("OPT:");
                foreach (var item in Opts.Parsers
                    .Concat(Opts.Parsers2)
                    .GroupJoin(Helper.ShortCutWithValue,
                    (opt) => opt.Name(),
                    (shortcut) => shortcut.Value,
                    resultSelector: (opt, shortcuts) =>
                    new {
                        Opt = opt,
                        Shortcut = shortcuts
                        .Select((it) => it.Key)
                        .FirstOrDefault()
                    }))
                {
                    var shortcut = string.IsNullOrEmpty(item.Shortcut)
                        ? "   " : $"{item.Shortcut},";
                    Console.WriteLine($"  {shortcut}{item.Opt}");
                }

                Console.WriteLine("Shortcut:");
                foreach (var shortcut in Helper.ShortCutWithoutValue)
                {
                    Console.WriteLine($"  {shortcut.Key} => {string.Join("\t", shortcut.Value)}");
                }

                Console.WriteLine();
                Console.Write($"Options defined by envir var '{nameof(dir2)}'");
                Console.Write(" will be parsed before command line opt.");
                Console.WriteLine();
                return true;
            }

            if (args.Contains("-?"))
            {
                Console.WriteLine("Syntax: dir2 --help");
                Console.WriteLine("Syntax: dir2 --version | -v");
                Console.WriteLine(
                    $"Syntax: dir2 DIR{Path.DirectorySeparatorChar}WILD [OPT ..]");
                Console.WriteLine("Syntax: dir2 [DIR] [WILD ..] [OPT ..]");
                Console.WriteLine("OPT:");
                foreach (var item in Opts.BriefParsers
                    .GroupJoin(Helper.ShortCutWithValue,
                    (opt) => opt.Name(),
                    (shortcut) => shortcut.Value,
                    resultSelector: (opt, shortcuts) =>
                    new {
                        Opt = opt,
                        Shortcut = shortcuts
                        .Select((it) => it.Key)
                        .FirstOrDefault()
                    }))
                {
                    var shortcut = string.IsNullOrEmpty(item.Shortcut)
                        ? "   " : $"{item.Shortcut},";
                    Console.WriteLine($"  {shortcut}{item.Opt}");
                }
                Console.WriteLine("Shortcut:");
                foreach (var shortcut in Opts.BriefShortCutWithoutValue)
                {
                    Console.WriteLine(
                        $"  {shortcut.Key} => {shortcut.Value}");
                }
                return true;
            }

            if (args.Contains("--help=cfg"))
            {
                Console.WriteLine($"Load opt from '{Config.GetFilename()}':");
                foreach (var opt in Opts.ConfigParsers)
                {
                    Console.WriteLine(opt);
                }
                foreach (var opt in Opts.ConfigParsers2)
                {
                    Console.WriteLine(opt);
                }
                Console.WriteLine();
                Console.WriteLine($"Apply opt '{Opts.ConfigFileOffOption}'");
                Console.WriteLine(" to skip the above config.");
                return true;
            }

            var helpPrefix = "--help=";
            var helpRequest = args
                .Where((it) => it.StartsWith(helpPrefix))
                .Select((it) => it.Substring(helpPrefix.Length))
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(helpRequest))
            {
                PrintHelpContent(helpRequest);
                return true;
            }

            return false;
        }

        private static bool PrintHelpContent(string helpRequest)
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;

            Func<string, string> NormalizeForDirSepChar = (it) => it;
            if (Path.DirectorySeparatorChar != '/')
            {
                NormalizeForDirSepChar = (it) => it.Replace('/',
                    Path.DirectorySeparatorChar);
            }

            var helpBasePath = Path.Join(Path.GetDirectoryName(
                NormalizeForDirSepChar(codeBase)), "help");

            var recdirectDefFilename = Path.Join(helpBasePath, "dir2-redir.txt");
            if (!File.Exists(recdirectDefFilename))
            {
                Console.Error.WriteLine($"File '{recdirectDefFilename}' is NOT found!");
                return false;
            }

            var redirectFilename = string.Empty;
            var questThe = $"{helpRequest}=";
            try
            {
                using (var fs = File.OpenText(recdirectDefFilename))
                {
                    redirectFilename = fs.ReadToEnd()
                        .Split(new char[] { '\n', '\r' })
                        .Where((it) => !string.IsNullOrEmpty(it))
                        .Select((it) => it.Trim())
                        .Where((it) => it.StartsWith(questThe))
                        .Select((it) => it.Substring(questThe.Length))
                        .FirstOrDefault();
                }
            }
            catch
            {
                Console.Error.WriteLine(
                    $"File '{recdirectDefFilename}': Some error is encountered!");
                return false;
            }

            if (string.IsNullOrEmpty(redirectFilename))
            {
                Console.WriteLine($"'{questThe.TrimEnd('=')}' is NOT defined in '{recdirectDefFilename}'");
                return false;
            }

            var theFilename = Path.Join(helpBasePath, redirectFilename);
            if (!File.Exists(theFilename))
            {
                Console.Error.WriteLine($"File '{theFilename}' is NOT found!");
                return false;
            }
            try
            {
                using (var fs = File.OpenText(theFilename))
                {
                    foreach (var line in fs.ReadToEnd()
                        .Split(new char[] { '\n', '\r' })
                        .Where((it) => !string.IsNullOrEmpty(it)))
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine($"File {theFilename}: {ee.Message}");
            }

            return true;
        }
    }
}
