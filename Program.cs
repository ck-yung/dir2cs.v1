using System;
using System.IO;

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

            var exclFileExtension = string.Empty;
            var exclFileExtensionOpt = "--excl-ext=";
            foreach (var arg in args)
            {
                if (!arg.StartsWith(exclFileExtensionOpt)) continue;
                exclFileExtension = arg.Substring(exclFileExtensionOpt.Length);
            }

            var count = 0;
            foreach (var filename in Helper.GetAllFiles(baseDir))
            {
                if (!string.IsNullOrEmpty(exclFileExtension))
                {
                    if (filename.EndsWith(exclFileExtension))
                    {
                        continue;
                    }
                }

                var info = new FileInfo(filename);
                Console.Write($"{info.Length,8} ");
                Console.Write($"{info.LastWriteTime:yyyy-MM-dd HH:mm:ss} ");
                Console.WriteLine(info.FullName);

                count += 1;
            }
            Console.WriteLine($"{count} files are found.");

            return;
        }
    }
}
