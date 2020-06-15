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

        static void MainRun(string[] args)
        {
            var baseDir = Directory.GetCurrentDirectory();

            foreach (var arg in args)
            {
                if (arg == "--create-date")
                {
                    GetFileDate = (it) => it.CreationTime;
                }
            }

            foreach (var opt in Opts.Parsers)
            {
                opt.Parse(args);
            }

            var sum = Helper.GetAllFiles(baseDir)
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
                .Where((it) => Opts.MaxFileSizeFilter.Func(it.Length))
                .Select((it) =>
                {
                    Console.Write(Opts.ItemText(it.ToString()));
                    return it;
                })
                .Aggregate(new InfoSum(),
                (acc, it) => acc.AddWith(it));

            Console.Write(Opts.TotalText($"{sum}{baseDir}"));

            return;
        }

        static public Func<FileInfo, DateTime> GetFileDate
        { get; private set; } = (it) => it.LastWriteTime;
    }
}
