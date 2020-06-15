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

            var exclFileExtensionOpt = "--excl-ext=";
            foreach (var arg in args)
            {
                if (!arg.StartsWith(exclFileExtensionOpt)) continue;
                exclFileExtension = arg.Substring(exclFileExtensionOpt.Length);
            }

            var filenameForExclExt = Helper.GetAllFiles(baseDir)
                .Where(includingFileExt);

            var fileInfos = filenameForExclExt
                .Select(ToFileInfo);

            var fileInfos2 = fileInfos
                .Select(PrintFileInfo);

            Console.WriteLine($"{fileInfos2.Count()} files are found.");

            return;
        }

        static bool includingFileExt(string filename)
        {
            if (string.IsNullOrEmpty(exclFileExtension))
            {
                return true;
            }
            return !filename.EndsWith(exclFileExtension);
        }

        static string exclFileExtension = string.Empty;

        static FileInfo ToFileInfo(string filename)
        {
            return new FileInfo(filename);
        }

        static FileInfo PrintFileInfo(FileInfo info)
        {
            Console.Write($"{info.Length,7} ");
            Console.Write($"{info.LastWriteTime:yyyy-MM-dd HH:mm:ss} ");
            Console.WriteLine(info.FullName);
            return info;
        }
    }
}
