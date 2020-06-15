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
            Console.WriteLine(baseDir);

            Console.WriteLine();
            Console.WriteLine("Directories::");
            foreach (var dirName in Directory.EnumerateDirectories(baseDir))
            {
                Console.WriteLine(dirName);
            }

            Console.WriteLine();
            Console.WriteLine("Files::");
            foreach (var fileName in Directory.EnumerateFiles(baseDir))
            {
                Console.WriteLine(fileName);
            }

            return;
        }
    }
}
