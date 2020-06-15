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

            Func<string, bool> IsIncludingFilename = (_) => true;
            var exclFileExtensionOpt = "--excl-ext=";
            foreach (var arg in args)
            {
                if (!arg.StartsWith(exclFileExtensionOpt)) continue;
                var exclFileExtension = arg.Substring(exclFileExtensionOpt.Length);
                IsIncludingFilename = (it) => !it.EndsWith(exclFileExtension);
            }

            var sum = new InfoSum();
            foreach (var info in Helper.GetAllFiles(baseDir)
                .Where((it) => IsIncludingFilename(it))
                .Select((it) => new FileInfo(it))
                .Select((it) =>
                {
                    Console.Write($"{it.Length,7} ");
                    Console.Write($"{it.LastWriteTime:yyyy-MM-dd HH:mm:ss} ");
                    Console.WriteLine(it.FullName);
                    return it;
                }))
            {
                sum.AddWith(info);
            }

            Console.WriteLine($"{sum} {baseDir}");

            return;
        }
    }
}
