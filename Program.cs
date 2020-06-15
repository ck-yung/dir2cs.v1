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

            var sum = Helper.GetAllFiles(baseDir)
                .Select((it) => InfoFile.From(it))
                .Where((it) => it.IsNotNone)
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
    }
}
