using System;
using System.IO;
using System.Linq;

namespace dir2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.InputEncoding = System.Text.Encoding.UTF8;
                MainRun(args);
            }
            catch (TooManyValuesException tmve)
            {
                Console.WriteLine($"Too many value to {tmve.Message}");
            }
            catch (InvalidValueException ive)
            {
                Console.WriteLine($"{ive.Message} is invalid");
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
            }
            catch (Exception ee)
            {
                Console.WriteLine();
                Console.WriteLine(ee);
            }
        }

        static void MainRun(string[] argsMain)
        {
            if (argsMain.Contains("-?"))
            {
                Console.WriteLine(
                    $"Syntax: dir2 DIR{Path.DirectorySeparatorChar}WILD [OPT ..]");
                Console.WriteLine("Syntax: dir2 [DIR] [WILD ..] [OPT ..]");
                Console.WriteLine("OPT:");
                foreach (var opt in Opts.Parsers)
                {
                    Console.WriteLine(opt);
                }
                return;
            }

            var args = Opts.Parsers
                .Aggregate(Opts.LoadConfig(argsMain),
                (it, opt) => opt.Parse(it))
                .ToArray();

            var baseDir = Directory.GetCurrentDirectory();

            if (args.Length > 0 && Directory.Exists(args[0]))
            {
                baseDir = Path.GetFullPath(args[0]);
                args = args.Skip(1).ToArray();
            }
            else if (args.Length == 1)
            {
                var dirThe = Path.GetDirectoryName(args[0]);
                if (!string.IsNullOrEmpty(dirThe))
                {
                    baseDir = Path.GetFullPath(dirThe);
                    args = new string[] { Path.GetFileName(args[0]) };
                }
            }

            InfoFile.InitDir(baseDir);

            var sum = Opts.GetFiles.Func(baseDir)
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
                .Where((it) => Opts.FilenameFilter.Func(it.Filename))
                .Where((it) =>!Opts.ExclFilenameFilter.Func(it.Filename))
                .Where((it) => Opts.MinFileSizeFilter.Func(it.Length))
                .Where((it) => Opts.MaxFileSizeFilter.Func(it.Length))
                .Where((it) => Opts.MinFileDateFilter.Func(it.DateTime))
                .Where((it) => Opts.MaxFileDateFilter.Func(it.DateTime))
                .Where((it) => Opts.FileExtFilter.Func(it.Filename))
                .Where((it) => Opts.HiddenFilter.Func(it))
                .Invoke(Opts.SumBy);

            if (sum.AddCount == 0)
                Console.Write(Opts.TotalText("No file is found."));
            else if (sum.AddCount > 1)
                Console.Write(Opts.TotalText(sum.ToString()));

            foreach (var arg in args)
                Console.WriteLine($"Unknown opt '{arg}'");

            return;
        }
    }
}
