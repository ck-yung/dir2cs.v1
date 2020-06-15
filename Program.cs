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
            catch (Exception ee)
            {
                Console.WriteLine();
                Console.WriteLine(ee);
            }
        }

        static void MainRun(string[] args)
        {
            var baseDir = Directory.GetCurrentDirectory();

            int? sizeWithin = null;
            var sizeWithinOpt = "--size-within=";
            var hideOpt = "--hide=";
            foreach (var arg in args)
            {
                if (arg == "--create-date")
                {
                    IsCreateDate = true;
                }
                else if (arg.StartsWith(sizeWithinOpt))
                {
                    if (int.TryParse(arg.Substring(sizeWithinOpt.Length),
                        out int intTemp))
                    {
                        sizeWithin = intTemp;
                    }
                }
                else if (arg.StartsWith(hideOpt))
                {
                    var valueThe = arg.Substring(hideOpt.Length);
                    switch (valueThe)
                    {
                        case "size":
                            IsPrintSize = false;
                            break;
                        case "date":
                            IsPrintDate = false;
                            break;
                        default:
                            break;
                    }
                }
            }

            var sum = Helper.GetAllFiles(baseDir)
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
                .Where((it) => (sizeWithin == null)
                ? true : (sizeWithin > it.Length))
                .Select((it) =>
                {
                    Console.WriteLine(it);
                    return it;
                })
                .Aggregate(new InfoSum(),
                (acc, it) => acc.AddWith(it));

            Console.WriteLine($"{sum} {baseDir}");

            return;
        }

        static public bool IsCreateDate { get; private set; } = false;
        static public bool IsPrintSize { get; private set; } = true;
        static public bool IsPrintDate { get; private set; } = true;
    }
}
