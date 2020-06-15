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

            var exclFileExtension = ".cache";
            var count = 0;
            foreach (var filename in Helper.GetAllFiles(baseDir))
            {
                if (filename.EndsWith(exclFileExtension))
                {
                    continue;
                }
                Console.WriteLine(filename);
                count += 1;
            }
            Console.WriteLine($"{count} files are found.");

            return;
        }
    }
}
