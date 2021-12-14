using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace dir2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MainRun(args);
            }
            catch (TooManyValuesException tmve)
            {
                Console.Error.WriteLine($"Too many value to {tmve.Message}");
            }
            catch (InvalidValueException ive)
            {
                Console.Error.WriteLine($"{ive.Message} is invalid");
            }
            catch (ArgumentException ae)
            {
                Console.Error.WriteLine(ae.Message);
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(ee);
            }
        }

        static void MainRun(string[] argsMain)
        {
            if (OnlineHelp.IsShow(argsMain))
            {
                return;
            }

            var envirOpts = Opts.GetEnvirOpts().EnvirExpandShortcut();

            var cmdOpts = Opts.LoadConfig(argsMain).ExpandShortcut();

            IEnumerable<string> envirExclOpts =
                Helper.EnvirParse(envirOpts);

            var args = Opts.Parsers
                .Aggregate(cmdOpts.Union(envirExclOpts),
                (it, opt) => opt.Parse(it))
                .ToImmutableArray();

            Opts.EncodeConsoleOuput.Func(true);

            var baseDir = Directory.GetCurrentDirectory();

            if (args.Length > 0 && Directory.Exists(args[0]))
            {
                baseDir = Path.GetFullPath(args[0]);
                args = args.Skip(1).ToImmutableArray();
            }
            else if (args.Length == 1)
            {
                var dirThe = Path.GetDirectoryName(args[0]);
                if (!string.IsNullOrEmpty(dirThe) &&
                    Directory.Exists(dirThe))
                {
                    baseDir = Path.GetFullPath(dirThe);
                    args = ImmutableArray.Create(
                        new string[] { Path.GetFileName(args[0]) });
                }
            }

            args = Opts.ParseFilenameFilter(args);

            InfoFile.InitDir(baseDir);

            var sum = Opts.GetFiles.Func(baseDir)
                .Where((it) =>
                {
                    var filename = Path.GetFileName(it);
                    return Opts.FilenameFilter.Func(filename)
                    && Opts.FileExtFilter.Func(filename)
                    && !Opts.ExclFilenameFilter.Func(filename);
                })
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
                .Where((it) => Opts.MinFileSizeFilter.Func(it.Length))
                .Where((it) => Opts.MaxFileSizeFilter.Func(it.Length))
                .Where((it) => Opts.MinFileDateFilter.Func(it.DateTime))
                .Where((it) => Opts.MaxFileDateFilter.Func(it.DateTime))
                .Where((it) => Opts.HiddenFilter.Func(it))
                .Invoke(Opts.SumBy);

            if (sum.AddCount == 0)
                Console.Write(Opts.TotalText("No file is found."));
            else if (sum.AddCount == 1)
            {
                if (Opts.ItemText == Helper.Print.Off)
                {
                    StringBuilder totalThe = new();
                    totalThe.Append(Opts.SizeText(
                        Opts.SizeFormat.Func(sum.Length)));
                    totalThe.Append(Opts.DateText(
                        Opts.DateFormat.Func(sum.DateTime)));
                    if (sum.Count>1)
                    {
                        totalThe.Append(Opts.DateText("- "));
                        totalThe.Append(Opts.DateText(
                            Opts.DateFormat.Func(sum.Last)));
                        totalThe.Append(Opts.CountText("(count="));
                        totalThe.Append(Opts.CountText(
                            Opts.CountFormat.Func(sum.Count).TrimEnd()));
                        totalThe.Append(Opts.CountText(")"));
                    }
                    Console.Write(Opts.TotalText(totalThe.ToString()));
                }
            }
            else if (sum.AddCount > 1)
                Console.Write(Opts.TotalText(sum.ToString()));

            foreach (var arg in args)
                Console.Error.WriteLine($"Unknown opt '{arg}'");

            return;
        }
    }
}
