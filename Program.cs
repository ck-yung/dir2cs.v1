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

            var count = Helper.GetAllFiles(baseDir)
                .MyFilter(filterThe: (it) => IsIncludingFilename(it))
                .MyToFileInfo()
                .MyPrintFileInfo(funcThe: (it) =>
                {
                    Console.Write($"{it.Length,8} ");
                    Console.Write($"{it.LastWriteTime:yyyy-MM-dd HH:mm:ss} ");
                    Console.WriteLine(it.FullName);
                    return it;
                })
                .MyCount();

            Console.WriteLine($"{count} files are found.");

            return;
        }
    }

    static public class MyTool
    {
        static public IEnumerable<string> MyFilter(
            this IEnumerable<string> seqThe, Func<string, bool> filterThe)
        {
            var enumThe = seqThe.GetEnumerator();
            while (enumThe.MoveNext())
            {
                var currentThe = enumThe.Current;
                if (!filterThe(currentThe)) continue;
                yield return currentThe;
            }
        }

        static public IEnumerable<FileInfo> MyToFileInfo(
            this IEnumerable<string> seqThe)
        {
            var enumThe = seqThe.GetEnumerator();
            while (enumThe.MoveNext())
            {
                var currentThe = enumThe.Current;
                yield return new FileInfo(currentThe);
            }
        }

        static public IEnumerable<FileInfo> MyPrintFileInfo(
            this IEnumerable<FileInfo> seqThe, Func<FileInfo, FileInfo> funcThe)
        {
            var enumThe = seqThe.GetEnumerator();
            while (enumThe.MoveNext())
            {
                var currentThe = enumThe.Current;
                yield return funcThe(currentThe);
            }
        }

        static public int MyCount(this IEnumerable<FileInfo> seqThe)
        {
            int result = 0;
            var enumThe = seqThe.GetEnumerator();
            while (enumThe.MoveNext())
            {
                result += 1;
            }
            return result;
        }
    }
}
