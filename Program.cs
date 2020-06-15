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

            var exclFileExtensionOpt = "--excl-ext=";
            foreach (var arg in args)
            {
                if (!arg.StartsWith(exclFileExtensionOpt)) continue;
                exclFileExtension = arg.Substring(exclFileExtensionOpt.Length);
            }

            var filenameForExclExt = new List<string>();
            foreach (var filename in Helper.GetAllFiles(baseDir))
            {
                if (!includingFileExt(filename)) continue;
                filenameForExclExt.Add(filename);
            }

            var fileInfos = new List<FileInfo>();
            foreach (var filename in filenameForExclExt)
            {
                fileInfos.Add(new FileInfo(filename));
            }

            foreach (var info in fileInfos)
            {
                Console.Write($"{info.Length,8} ");
                Console.Write($"{info.LastWriteTime:yyyy-MM-dd HH:mm:ss} ");
                Console.WriteLine(info.FullName);
            }
            Console.WriteLine($"{fileInfos.Count} files are found.");

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
    }
}
