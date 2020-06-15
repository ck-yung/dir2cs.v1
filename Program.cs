using System;
using System.Collections.Generic;
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

            Func<string, bool> IsIncludingFilename = (_) => true;
            var exclFileExtensionOpt = "--excl-ext=";
            foreach (var arg in args)
            {
                if (!arg.StartsWith(exclFileExtensionOpt)) continue;
                var exclFileExtension = arg.Substring(exclFileExtensionOpt.Length);
                IsIncludingFilename = (it) => !it.EndsWith(exclFileExtension);
            }

            var filterResult = MyFilter(
                Helper.GetAllFiles(baseDir),
                filterThe: (it) => IsIncludingFilename(it));

            var toFileInfoResult = MyToFileInfo(filterResult);

            var count = 0;
            foreach (var info in toFileInfoResult)
            {
                Console.Write($"{info.Length,8} ");
                Console.WriteLine(info.FullName);
                count += 1;
            }
            Console.WriteLine($"{count} files are found.");

            return;
        }

        static IEnumerable<string> MyFilter(
            IEnumerable<string> seqThe, Func<string, bool> filterThe)
        {
            var enumThe = seqThe.GetEnumerator();
            while (enumThe.MoveNext())
            {
                var currentThe = enumThe.Current;
                if (!filterThe(currentThe)) continue;
                yield return currentThe;
            }
        }

        static IEnumerable<FileInfo> MyToFileInfo(
            IEnumerable<string> seqThe)
        {
            var enumThe = seqThe.GetEnumerator();
            while (enumThe.MoveNext())
            {
                var currentThe = enumThe.Current;
                yield return new FileInfo(currentThe);
            }
        }
    }
}
