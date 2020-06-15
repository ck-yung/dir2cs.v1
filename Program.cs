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

            var hideOpt = "--hide=";
            var totalOpt = "--total=";
            foreach (var arg in args)
            {
                if (arg == "--create-date")
                {
                    GetFileDate = (it) => it.CreationTime;
                }
                else if (arg.StartsWith(hideOpt))
                {
                    var valueThe = arg.Substring(hideOpt.Length);
                    switch (valueThe)
                    {
                        case "size":
                            SizeText = (_) => "";
                            break;
                        case "date":
                            DateText = (_) => "";
                            break;
                        default:
                            break;
                    }
                }
                else if (arg.StartsWith(totalOpt))
                {
                    var valueThe = arg.Substring(totalOpt.Length);
                    switch (valueThe)
                    {
                        case "off":
                            TotalText = (_) => "";
                            break;
                        case "only":
                            ItemText = (_) => "";
                            break;
                        default:
                            break;
                    }
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
                    Console.Write(ItemText(it.ToString()));
                    return it;
                })
                .Aggregate(new InfoSum(),
                (acc, it) => acc.AddWith(it));

            Console.Write(TotalText($"{sum} {baseDir}"));

            return;
        }

        static public Func<FileInfo, DateTime> GetFileDate
        { get; private set; } = (it) => it.LastWriteTime;
        static public Func<string, string> SizeText
        { get; private set; } = (it) => it;
        static public Func<string, string> DateText
        { get; private set; } = (it) => it;
        static public Func<string, string> ItemText
        { get; private set; } = (it) => it + Environment.NewLine;
        static public Func<string, string> TotalText
        { get; private set; } = (it) => it + Environment.NewLine;
    }
}
