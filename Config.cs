using System;
using System.Collections.Generic;
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

        static public IEnumerable<string> ParseFile()
        {
            try
            {
                var cfgFilename = GetFilename();
                using var fs = File.OpenText(cfgFilename);
                var lines = fs.ReadToEnd()
                    .Split('\n', '\r')
                    .Select((it) => it.Trim())
                    .Where((it) => !string.IsNullOrEmpty(it));
                try
                {
                    return Opts.ConfigParsers
                        .Aggregate(lines, (acc, opt) => opt.Parse(acc))
                        .Join(Opts.ConfigParsers2,
                        outerKeySelector: (line) => line.Split('=')[0],
                        innerKeySelector: (opt) => opt.Name().Trim('='),
                        resultSelector: (line, opt) => line);
                }
                catch (Exception ee)
                {
                    Console.Error.WriteLine(
                        $"Config file {cfgFilename} [{ee.GetType()}] {ee.Message}");
                    Console.Error.WriteLine();
                    return Helper.emptyStrings;
                }
            }
            catch
            {
                return Helper.emptyStrings;
            }
        }

        static internal Tuple<bool,IEnumerable<string>>
            PreParseEnvir()
        {
            var envirOld = Environment.GetEnvironmentVariable(
                nameof(dir2));
            if (string.IsNullOrEmpty(envirOld))
            {
                return new Tuple<bool,IEnumerable<string>>(
                    false, Array.Empty<string>());
            }

            var envirCfgOffCheck = (" "+envirOld)
                .Split(" --")
                .Select((it) => it.Trim())
                .Where((it) => it.Length> 0)
                .Distinct()
                .Select((it) => "--"+ it)
                .GroupBy((it) => it.Equals("--cfg-off"))
                .ToDictionary((grp)=>grp.Key,(grp)=>grp)
                ;

            IEnumerable<string> envirOthers =
                envirCfgOffCheck.ContainsKey(false)
                ? envirCfgOffCheck[false]
                : Array.Empty<string>();

            return new Tuple<bool, IEnumerable<string>>(
                envirCfgOffCheck.ContainsKey(true),
                envirOthers);
        }
    }
}
