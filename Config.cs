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

        static public string[] ParseFile()
        {
            try
            {
                var cfgFilename = GetFilename();
                using (var fs = File.OpenText(cfgFilename))
                {
                    var lines = fs.ReadToEnd()
                        .Split('\n', '\r')
                        .Select((it) => it.Trim())
                        .Where((it) => !string.IsNullOrEmpty(it))
                        .ToArray();
                    try
                    {
                        return Opts.ConfigParsers
                            .Aggregate(lines, (acc, opt) => opt.Parse(acc))
                            .ToArray();
                    }
                    catch (Exception ee)
                    {
                        Console.Error.WriteLine(
                            $"Config file {cfgFilename} [{ee.GetType()}] {ee.Message}");
                        Console.Error.WriteLine();
                        return Helper.emptyStrings;
                    }
                }
            }
            catch
            {
                return Helper.emptyStrings;
            }
        }
    }
}