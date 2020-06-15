using System;
using System.Collections.Generic;
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
            Console.WriteLine($"{baseDir}:");

            Func<string, bool> IsIncludingFilename = (_) => true;
            var exclFileExtensionOpt = "--excl-ext=";
            foreach (var arg in args)
            {
                if (!arg.StartsWith(exclFileExtensionOpt)) continue;
                var exclFileExtension = arg.Substring(exclFileExtensionOpt.Length);
                IsIncludingFilename = (it) => !it.EndsWith(exclFileExtension);
            }

            var count = Helper.GetAllFiles(baseDir)
                .Where((filename) => IsIncludingFilename(filename))
                .Select((filename) => new FileInfo(filename))
                .Select((info) =>
                {
                    Console.Write($"{info.Length,7} ");
                    Console.Write($"{info.LastWriteTime:yyyy-MM-dd HH:mm:ss} ");
                    Console.WriteLine(info.FullName);
                    return info;
                })
                .Zip(Make.Strings(),
                    resultSelector: (info, it) =>
                    {
                        Console.Write(it);
                        return info;
                    })
                .Count();

            Console.WriteLine($"{count} files are found.");

            return;
        }
    }

    static class Make
    {
        static public IEnumerable<string> Strings()
        {
            while (true)
            {
                yield return "";
                yield return "";
                yield return "";
                yield return "";
                yield return Environment.NewLine;
            }
        }
    }
}
