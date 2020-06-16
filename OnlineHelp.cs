using System;
using System.IO;
using System.Linq;

namespace dir2
{
    static class OnlineHelp
    {
        static public bool IsShow(string[] args)
        {
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
                return true;
            }

            if (args.Contains("-?"))
            {
                Console.WriteLine("Syntax: dir2 --help");
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
                Console.WriteLine("Apply opt '--cfg-off' to skip the above config.");
                return true;
            }

            return false;
        }
    }
}
