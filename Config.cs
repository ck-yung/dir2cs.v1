using System;
using System.IO;
using System.Linq;

namespace dir2
{
    static class Config
    {
        static public string GetFilename()
        {
            var pathHome = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(pathHome))
            {
                pathHome = Environment.GetEnvironmentVariable("UserProfile");
            }
            return Path.Join(pathHome, ".local", "dir2.opt");
        }

        static public void ReadFile()
        {
            try
            {
                var cfgFilename = GetFilename();
                Console.WriteLine($"*** Content of '{cfgFilename}':");
                using (var fs = File.OpenText(cfgFilename))
                {
                    var lines = fs.ReadToEnd()
                        .Split('\n', '\r')
                        .Select((it) => it.Trim())
                        .Where((it) => !string.IsNullOrEmpty(it));
                    foreach (var line in lines)
                    {
                        Console.WriteLine(line);
                    }
                }
                Console.WriteLine();
            }
            catch
            {
                // do nothing
            }
        }
    }
}