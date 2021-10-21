using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace dir2
{
    static partial class Opts
    {
        static public Func<string, Regex> MakeRegex { get; private set; }
        = (it) => new Regex(it, RegexOptions.IgnoreCase);

        static readonly IFunc<bool, IEnumerable<string>> LoadConfigOpt =
            new Switcher<bool, IEnumerable<string>>(Opts.ConfigFileOffOption,
                help: "see --help=cfg",
                invoke: (_) => Config.ParseFile(), alt: (_) => Helper.emptyStrings);

        static public IEnumerable<string> LoadConfig(IEnumerable<string> args)
        {
            var parserThe = (IParser)LoadConfigOpt;
            var parsedResult = parserThe.Parse(args);
            return LoadConfigOpt.Func(true).Concat(parsedResult);
        }

        static internal IEnumerable<string> GetEnvirOpts()
        {
            var parserThe = (IParser)LoadConfigOpt;
            return parserThe.Parse(Config.GetEnvirOpts());
        }

        static public readonly IFunc<string, string> CaseOpt =
            new Switcher<string, string>("--case-sensitive",
                invoke: (it) => it.ToLower(), alt: (it) => it,
                postAlt: (opt) =>
                {
                    MakeRegex = (it) => new Regex(it, RegexOptions.None);
                });

        static public readonly IFunc<string, bool> FilenameFilter =
            new Function2<string, bool>("--name=", help: "WILD[,WILD,..]",
                invoke: (_) => true,
                parse: (opt, args) =>
                {
                    var exclNames = args
                    .Select((it) => Helper.ToWildMatch(it))
                    .ToImmutableArray();

                    opt._Invoke = (filename) => exclNames
                    .Any((wildMatch) => wildMatch(filename));
                });

        static public ImmutableArray<string>
            ParseFilenameFilter(IEnumerable<string> args)
        {
            var parser = (IParser)FilenameFilter;
            return parser.Parse(
                args
                .Select((it) =>
                it.StartsWith("-") ? it : $"--name={it}")
                ).ToImmutableArray();
        }

        static public readonly IFunc<string, bool> ExclFilenameFilter =
            new Function2<string, bool>("--excl-file=",
                help: "WILD[,WILD,..]", invoke: (_) => false,
                parse: (opt, args) =>
                {
                    var filterThe = args
                    .Select((it) => Helper.ToWildMatch(it))
                    .ToImmutableArray();

                    if (filterThe.Length>0)
                    {
                        opt._Invoke = (filename) => filterThe
                        .Any((it) => it(filename));
                    }
                });

        static public readonly IFunc<string, bool> ExclDirnameFilter =
            new Function2<string, bool>("--excl-dir=",
                help: "WILD[,WILD,..]", invoke: (_) => false,
                parse: (opt, args) =>
                {
                    var filterThe = args
                    .Select((it) => Helper.ToWildMatch(it))
                    .ToImmutableArray();

                    if (filterThe.Length > 0)
                    {
                        opt._Invoke = (filename) =>
                        filterThe.Any((it) => it(filename));
                    }
                });

        static public readonly IFunc<long, bool> MinFileSizeFilter =
            new Function<long, bool>("--size-beyond=",
                help: "NUMBER",
                invoke: (_) => true,
                parse: (opt, arg) =>
                {
                    if (Helper.TryParseAsLong(arg,
                        out long longThe) && (longThe >= 0))
                    {
                        opt._Invoke = (it) => it >= longThe;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<long, bool> MaxFileSizeFilter =
            new Function<long, bool>("--size-within=",
                help: "NUMBER",
                invoke: (_) => true,
                parse: (opt, arg) =>
                {
                    if (Helper.TryParseAsLong(arg,
                        out long longThe) && (longThe >= 0))
                    {
                        opt._Invoke = (it) => longThe > it;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<DateTime, bool> MinFileDateFilter =
            new Function<DateTime, bool>("--date-beyond=", help: "DATETIME",
                invoke: (_) => true, parse: (opt, arg) =>
                {
                    if (Helper.TryParseDateTime(arg, out DateTime result))
                    {
                        opt._Invoke = (it) => result > it;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<DateTime, bool> MaxFileDateFilter =
            new Function<DateTime, bool>("--date-within=", help: "DATETIME",
                invoke: (_) => true, parse: (opt, arg) =>
                {
                    if (Helper.TryParseDateTime(arg, out DateTime result))
                    {
                        opt._Invoke = (it) => it >= result;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<string, bool> FileExtFilter =
            new Function<string, bool>("--no-ext=", help: "excl|only",
                invoke: (_) => true,
                parse: (opt, arg) =>
                {
                    opt._Invoke = arg switch
                    {
                        "excl" => (it) =>
                        !string.IsNullOrEmpty(Path.GetExtension(it)),
                        "only" => (it) =>
                        string.IsNullOrEmpty(Path.GetExtension(it)),
                        _ => throw new InvalidValueException(
                            arg, opt.Name()),
                    };
                });

        static public readonly IFunc<InfoFile, bool> HiddenFilter =
            new Function<InfoFile, bool>("--hidden=", help: "incl|only",
                invoke: (it) => !it.IsHidden,
                parse: (opt, arg) =>
                {
                    opt._Invoke = arg switch
                    {
                        "incl" => (_) => true,
                        "only" => (it) => it.IsHidden,
                        _ => throw new InvalidValueException(
                            arg, opt.Name()),
                    };
                });

        static public Func<string, string> ItemText
        { get; private set; } = (it) => $"{it}{Environment.NewLine}";
        static public Func<string, string> TotalText
        { get; private set; } = (it) => $"{it}{Environment.NewLine}";
        static public readonly IParser TotalOpt = new Parser("--total=",
            help: "off|only",
            parse: (opt, arg) =>
            {
                switch (arg)
                {
                    case "off":
                        TotalText = (_) => "";
                        break;
                    case "only":
                        ItemText = (_) => "";
                        break;
                    default:
                        throw new InvalidValueException(
                            arg, opt.Name());
                }
            });

        static public Func<string, string> SizeText
        { get; private set; } = (it) => it;
        static public Func<string, string> DateText
        { get; private set; } = (it) => it;
        static public Func<string, string> CountText
        { get; private set; } = (it) => it;

        static public Func<string, string> DirNameText
        { get; private set; } = (it) => $"[Dir] {it}";

        static public readonly IParser HideOpt = new Function2<bool,bool>("--hide=",
            help: "size,date,count", invoke: (_) => false,
            parse: (opt, args) =>
            {
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "size":
                            SizeText = (_) => "";
                            DirNameText = (it)
                            => it + Path.DirectorySeparatorChar;
                            break;
                        case "date":
                            DateText = (_) => "";
                            break;
                        case "count":
                            CountText = (_) => "";
                            break;
                        default:
                            throw new InvalidValueException(args[0], opt.Name());
                    }
                }
            });

        static public readonly IFunc<FileInfo,DateTime> GetFileDate =
            new Switcher<FileInfo, DateTime>("--create-date",
            invoke: (it) => it.LastWriteTime, alt: (it) => it.CreationTime);

        static public Func<IEnumerable<InfoFile>, IEnumerable<InfoFile>>
            SortFileInfo { get; private set; } = (seqThe) => seqThe;
        static public Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>>
            SortSumInfo { get; private set; } = (seqThe) => seqThe;

        static public readonly IParser SortOpt = new Parser(
            "--sort=", help: "name|size|date|last|count",
            parse: (opt, arg) =>
            {
                switch (arg)
                {
                    case "name":
                        SortFileInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.FullName);
                        SortSumInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.Name);
                        break;
                    case "size":
                        SortFileInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.Length);
                        SortSumInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.Length);
                        break;
                    case "date":
                        SortFileInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.DateTime);
                        SortSumInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.DateTime);
                        break;
                    case "last":
                        SortSumInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.Last);
                        break;
                    case "count":
                        SortSumInfo =
                        (seqThe) => seqThe.OrderBy((it) => it.Count);
                        break;
                    default:
                        throw new InvalidValueException(arg, opt.Name());
                }
            });

        static public readonly IFunc<IEnumerable<InfoFile>, InfoSum> SumBy =
            new Function<IEnumerable<InfoFile>, InfoSum>(
                "--sum=", help: "ext|dir|+dir",
                invoke: (seqThe) => seqThe
                .Invoke((seqThe) => SortFileInfo(seqThe))
                .Invoke((seqThe) => OrderOpt.Func(seqThe))
                .Select((it) =>
                {
                    Console.Write(ItemText(it.ToString()));
                    return it;
                })
                .Aggregate(new InfoSum(InfoFile.BaseDir),
                (acc, it) => acc.AddWith(it)),
                parse: (opt, arg) =>
                {
                    PrintDir = (_) => { };
                    opt._Invoke = arg switch
                    {
                        "ext" => (seqThe) => seqThe
                        .GroupBy((it) => CaseOpt.Func(
                            Path.GetExtension(it.Filename)))
                        .Select((grp) => grp.Aggregate(new InfoSum(
                            string.IsNullOrEmpty(grp.Key)
                            ? "*no-ext*" : grp.Key),
                        (acc, it) => acc.AddWith(it)))
                        .Invoke(SortSumInfo)
                        .Invoke(SumOrder)
                        .Select((it) =>
                        {
                            Console.Write(ItemText(it.ToString()));
                            return it;
                        })
                        .Aggregate(new InfoSum(InfoFile.BaseDir),
                        (acc, it) => acc.AddWith(it)),

                        "dir" => (seqThe) => seqThe
                        .GroupBy((it) => Helper.GetFirstPath(
                        InfoFile.RelativePath(it.FullName)))
                        .Select((grp) => grp.Aggregate(new InfoSum(grp.Key),
                        (acc, it) => acc.AddWith(it)))
                        .Invoke(SortSumInfo)
                        .Invoke(SumOrder)
                        .Select((it) =>
                        {
                            Console.Write(ItemText(it.ToString()));
                            return it;
                        })
                        .Aggregate(new InfoSum(InfoFile.BaseDir),
                        (acc, it) => acc.AddWith(it)),

                        "+dir" => (seqThe) =>
                        {
                            var qryScan = seqThe
                            .GroupBy((it) => Helper.GetFirstPath(
                                InfoFile.RelativePath(it.FullName)))
                            .Select((grp) => grp.Aggregate(
                                new InfoSum(grp.Key),
                                (acc, it) => acc.AddWith(it)));

                            var qryResult =
                            from dirDefault in (new string[1] { "." })
                            .AsEnumerable()
                            .Union(Directory.EnumerateDirectories(
                                InfoFile.BaseDir)
                            .Select(it => Path.GetFileName(it)))
                            join dirScan in qryScan
                            on dirDefault equals dirScan.Name
                            into joinThe
                            from dirFound in joinThe.DefaultIfEmpty()
                            select dirFound == null
                            ? new InfoSum(dirDefault, false) : dirFound;

                            return qryResult
                            .Invoke(SortSumInfo)
                            .Invoke(SumOrder)
                            .Select((it) =>
                            {
                                Console.Write(ItemText(it.ToString()));
                                return it;
                            })
                            .Aggregate(new InfoSum(InfoFile.BaseDir),
                            (acc, it) => acc.AddWith(it));
                        },

                    _ => throw new InvalidValueException(arg, opt.Name()),
                    };
                });

        static public readonly IFunc<string, Func<string, string>>
            MakeRelativePath = new Switcher<string, Func<string, string>>(
                "--relative", invoke: (dirname) =>
                {
                    var pathLen = dirname.Length;
                    if (!dirname.EndsWith(Path.DirectorySeparatorChar))
                        pathLen += 1;
                    return (it) => it[pathLen..];
                }, alt: (dirname) =>
                {
                    var currDir = Directory.GetCurrentDirectory();
                    if (!dirname.StartsWith(currDir))
                    {
                        throw new ArgumentException(
                            $"'--relative': '{dirname}' is NOT in current directory!");
                    }
                    var pathLen = currDir.Length;
                    if (!currDir.EndsWith(Path.DirectorySeparatorChar))
                        pathLen += 1;
                    return (it) => it[pathLen..];
                });

        static public readonly IParser[] Parsers = new IParser[]
        {
            (IParser) LoadConfigOpt,
            (IParser) EncodeConsoleOuput,
            (IParser) ToRegexText,
            (IParser) CaseOpt,
            (IParser) GetFileDate,
            (IParser) MakeRelativePath,
            (IParser) CountComma,
            (IParser) OrderOpt,
            (IParser) MinFileSizeFilter,
            (IParser) MaxFileSizeFilter,
            (IParser) MinFileDateFilter,
            (IParser) MaxFileDateFilter,
            (IParser) FileExtFilter,
            (IParser) HiddenFilter,
            (IParser) SizeFormat,
            (IParser) CountFormat,
            (IParser) DateFormat,
            TotalOpt,
            HideOpt,
            SortOpt,
            (IParser) GetFiles,
            (IParser) SumBy,
            (IParser) ExclFilenameFilter,
            (IParser) ExclDirnameFilter,
            (IParser) TakeOpt,
        };

        static public readonly IParser[] Parsers2 = new IParser[]
        {
            (IParser) FilenameFilter,
        };

        static public readonly IParser[] ConfigParsers = new IParser[]
        {
            (IParser) EncodeConsoleOuput,
            (IParser) ToRegexText,
            (IParser) CaseOpt,
            (IParser) OrderOpt,
            (IParser) CountComma,
            (IParser) HiddenFilter,
            (IParser) SizeFormat,
            (IParser) CountFormat,
            (IParser) DateFormat,
            SortOpt,
        };

        static public readonly IParser[] EnvirParsers = new IParser[]
        {
            (IParser) LoadConfigOpt,
            (IParser) EncodeConsoleOuput,
            (IParser) ToRegexText,
            (IParser) CaseOpt,
            (IParser) GetFileDate,
            (IParser) MakeRelativePath,
            (IParser) CountComma,
            (IParser) OrderOpt,
            (IParser) MinFileSizeFilter,
            (IParser) MaxFileSizeFilter,
            (IParser) MinFileDateFilter,
            (IParser) MaxFileDateFilter,
            (IParser) FileExtFilter,
            (IParser) HiddenFilter,
            (IParser) SizeFormat,
            (IParser) CountFormat,
            (IParser) DateFormat,
            TotalOpt,
            HideOpt,
            SortOpt,
            (IParser) GetFiles,
            (IParser) SumBy,
            (IParser) TakeOpt,
        };

        static public readonly IParser[] ExclFileDirParsers = new IParser[]
        {
            (IParser) ExclFilenameFilter,
            (IParser) ExclDirnameFilter,
        };

        static public readonly IParser[] BriefParsers = new IParser[]
        {
            (IParser) ExclFilenameFilter,
            (IParser) ExclDirnameFilter,
            (IParser) HiddenFilter,
            (IParser) SizeFormat,
            TotalOpt,
            SortOpt,
            (IParser) SumBy,
        };

        static public readonly ImmutableDictionary<string, string>
            BriefShortCutWithoutValue = new Dictionary<string, string>()
            {
                ["-s"] = "list file recursively",
                ["-f"] = "list file only (exclsive to '-s')",
                ["-b"] = "list file name only",
            }.ToImmutableDictionary();
    }
}
