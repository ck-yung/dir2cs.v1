﻿using System;
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

            Func<long, bool> maxFileSizeFilter = (_) => true;
            var sizeWithinOpt = "--size-within=";
            var hideOpt = "--hide=";
            var totalOpt = "--total=";
            foreach (var arg in args)
            {
                if (arg == "--create-date")
                {
                    GetFileDate = (it) => it.CreationTime;
                }
                else if (arg.StartsWith(sizeWithinOpt))
                {
                    if (int.TryParse(arg.Substring(sizeWithinOpt.Length),
                        out int intTemp))
                    {
                        maxFileSizeFilter = (it) => intTemp > it;
                    }
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
                            IsPrintTotal = false;
                            IsPrintItem = true;
                            break;
                        case "only":
                            IsPrintTotal = true;
                            IsPrintItem = false;
                            break;
                        default:
                            break;
                    }
                }
            }

            var sum = Helper.GetAllFiles(baseDir)
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
                .Where((it) => maxFileSizeFilter(it.Length))
                .Select((it) =>
                {
                    if (IsPrintItem) Console.WriteLine(it);
                    return it;
                })
                .Aggregate(new InfoSum(),
                (acc, it) => acc.AddWith(it));

            if (IsPrintTotal)
            {
                Console.WriteLine($"{sum} {baseDir}");
            }

            return;
        }

        static public Func<FileInfo, DateTime> GetFileDate
        { get; private set; } = (it) => it.LastWriteTime;
        static public Func<string, string> SizeText
        { get; private set; } = (it) => it;
        static public Func<string, string> DateText
        { get; private set; } = (it) => it;
        static public bool IsPrintItem { get; private set; } = true;
        static public bool IsPrintTotal { get; private set; } = true;
    }
}
