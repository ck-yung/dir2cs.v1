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
            if (args.Contains("-?"))
            {
                Console.WriteLine("Syntax: dir2 [DIR] [WILD ..] [opt ..]");
                foreach (var opt in Opts.Parsers)
                {
                    Console.WriteLine(opt);
                }
                return;
            }

            var baseDir = Directory.GetCurrentDirectory();

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
    }
}
