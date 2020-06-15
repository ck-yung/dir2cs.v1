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
                Console.WriteLine("Syntax: dir2 [DIR] [WILD ..] [opt ..]");
                foreach (var opt in Opts.Parsers)
                {
                    Console.WriteLine(opt);
                }
                return;
            }

            var baseDir = Directory.GetCurrentDirectory();

            var args = Opts.Parsers
                .Aggregate(argsMain,
                (it, opt) => opt.Parse(it));

            var sum = Helper.GetAllFiles(baseDir)
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
                .Where((it) => Opts.MaxFileSizeFilter.Func(it.Length))
                .GroupBy((it) => Path.GetExtension(it.Filename))
                .Select((grp) => grp.Aggregate(new InfoSum(),
                (acc, it) => acc.AddWith(it)))
                .Select((it) =>
                {
                    Console.Write(Opts.ItemText(it.ToString()));
                    return it;
                })
                .Aggregate(new InfoSum(),
                (acc, it) => acc.AddWith(it));

            if (sum.AddCount == 0)
                Console.Write(Opts.TotalText("No file is found."));
            else if (sum.AddCount > 1)
                Console.Write(Opts.TotalText($"{sum}{baseDir}"));

            return;
        }
    }
}
