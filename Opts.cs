using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace dir2
{
    static partial class Opts
    {
        static public Func<string, Regex> MakeRegex { get; private set; }
        = (it) => new Regex(it, RegexOptions.IgnoreCase);

        static public readonly IFunc<string, string> CaseOpt =
            new Switcher<string, string>("--case-sensitive",
                invoke: (it) => it.ToLower(), alt: (it) => it,
                postAlt: (opt) =>
                {
                    MakeRegex = (it) => new Regex(it, RegexOptions.None);
                });

        static public readonly IFunc<string, bool> ExclFilenameFilter =
            new Function2<string, bool>("--excl-file=",
                help: "WILD[,WILD,..]", invoke: (_) => false,
                parse: (opt, args) =>
                {
                    var filterThe = args
                    .Select((it) => Helper.ToWildMatch(it))
                    .ToArray();

                    if (filterThe.Length>0)
                    {
                        opt.invoke = (filename) => filterThe
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
                    .ToArray();

                    if (filterThe.Length > 0)
                    {
                        opt.invoke = (filename) =>
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
                        opt.invoke = (it) => it >= longThe;
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
                        opt.invoke = (it) => longThe > it;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<DateTime, bool> MinFileDateFilter =
            new Function<DateTime, bool>("--date-beyond=", help: "DATETIME",
                invoke: (_) => true, parse: (opt, arg) =>
                {
                    if (Helper.TryParseDateTime(arg, out DateTime result))
                    {
                        opt.invoke = (it) => result > it;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<DateTime, bool> MaxFileDateFilter =
            new Function<DateTime, bool>("--date-within=", help: "DATETIME",
                invoke: (_) => true, parse: (opt, arg) =>
                {
                    if (Helper.TryParseDateTime(arg, out DateTime result))
                    {
                        opt.invoke = (it) => it >= result;
                    }
                    else throw new InvalidValueException(arg, opt.Name());
                });

        static public readonly IFunc<string, bool> FileExtFilter =
            new Function<string, bool>("--no-ext=", help: "excl|only",
                invoke: (_) => true,
                parse: (opt, arg) =>
                {
                    switch (arg)
                    {
                        case "excl":
                            opt.invoke = (it) =>
                            !string.IsNullOrEmpty(Path.GetExtension(it));
                            break;
                        case "only":
                            opt.invoke = (it) =>
                            string.IsNullOrEmpty(Path.GetExtension(it));
                            break;
                        default:
                            throw new InvalidValueException(arg, opt.Name());
                    }
                });

        static public readonly IFunc<InfoFile, bool> HiddenFilter =
            new Function<InfoFile, bool>("--hidden=", help: "incl|only",
                invoke: (it) => !it.IsHidden,
                parse: (opt, arg) =>
                {
                    switch (arg)
                    {
                        case "incl":
                            opt.invoke = (_) => true;
                            break;
                        case "only":
                            opt.invoke = (it) => it.IsHidden;
                            break;
                        default:
                            throw new InvalidValueException(arg, opt.Name());
                    }
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
                "--sum=", help: "ext|dir",
                invoke: (seqThe) => seqThe
                .Invoke((seqThe) => SortFileInfo(seqThe))
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
                    switch (arg)
                    {
                        case "ext":
                            opt.invoke = (seqThe) => seqThe
                            .GroupBy((it) => CaseOpt.Func(
                                Path.GetExtension(it.Filename)))
                            .Select((grp) => grp.Aggregate(new InfoSum(
                                string.IsNullOrEmpty(grp.Key)
                                ? "*no-ext*" : grp.Key),
                            (acc, it) => acc.AddWith(it)))
                            .Invoke(SortSumInfo)
                            .Select((it) =>
                            {
                                Console.Write(ItemText(it.ToString()));
                                return it;
                            })
                            .Aggregate(new InfoSum(InfoFile.BaseDir),
                            (acc, it) => acc.AddWith(it));
                            break;
                        case "dir":
                            opt.invoke = (seqThe) => seqThe
                            .GroupBy((it) => Helper.GetFirstPath(
                                InfoFile.RelativePath(it.FullName)))
                            .Select((grp) => grp.Aggregate(new InfoSum(grp.Key),
                            (acc, it) => acc.AddWith(it)))
                            .Invoke(SortSumInfo)
                            .Select((it) =>
                            {
                                Console.Write(ItemText(it.ToString()));
                                return it;
                            })
                            .Aggregate(new InfoSum(InfoFile.BaseDir),
                            (acc, it) => acc.AddWith(it));
                            break;
                        default:
                            throw new InvalidValueException(arg, opt.Name());
                    }
                });

        static public readonly IFunc<string, Func<string, string>>
            MakeRelativePath = new Switcher<string, Func<string, string>>(
                "--relative", invoke: (dirname) =>
                {
                    var pathLen = dirname.Length;
                    if (!dirname.EndsWith(Path.DirectorySeparatorChar))
                        pathLen += 1;
                    return (it) => it.Substring(pathLen);
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
                    return (it) => it.Substring(pathLen);
                });

        static Action<string> PrintDir { get; set; } =
            (dirname) => Helper.PrintDir(dirname);

        static public readonly IFunc<string, IEnumerable<string>> GetFiles =
            new Function<string, IEnumerable<string>>(
                "--dir=", help: "sub|off|only",
                invoke: (dirname) =>
                {
                    PrintDir(dirname);
                    return Helper.GetFiles(dirname);
                },
                parse: (opt, arg) =>
                {
                    switch (arg)
                    {
                        case "sub":
                            opt.invoke = (dirname) => Helper.GetAllFiles(dirname);
                            break;
                        case "off":
                            PrintDir = (_) => { };
                            break;
                        case "only":
                            opt.invoke = (dirname) =>
                            {
                                PrintDir(dirname);
                                TotalText = (_) => "";
                                return Helper.emptyStrings;
                            };
                            break;
                        default:
                            throw new InvalidValueException(arg, opt.Name());
                    }
                });

        static public readonly IFunc<long, string> SizeFormat =
            new Function<long, string>("--size-format=", help: "long|short",
                invoke: (it) => $"{it,8} ",
                parse: (opt, arg) =>
                {
                    switch (arg)
                    {
                        case "long":
                            opt.invoke = (it) => $"{it,19:N0} ";
                            break;
                        case "short":
                            opt.invoke = (it) => $"{Helper.ToKiloUnit(it)} ";
                            break;
                        default:
                            throw new InvalidValueException(arg, opt.Name());
                    }
                });

        static public readonly IFunc<bool, string> CountComma =
            new Switcher<bool, string>("--count-comma",
                invoke: (_) => "", alt: (_) => ":N0",
                postAlt: (opt) =>
                {
                    ((IParser)CountFormat).Parse(
                        new string[] { "--count-width=5" });
                });

        static public readonly IFunc<int, string> CountFormat =
            new Function<int, string>("--count-width=",
                help: "NUMBER",
                invoke: (it) => $"{it,4} ",
                parse: (opt, arg) =>
                {
                    if (int.TryParse(arg, out int widthThe))
                    {
                        if (widthThe < 1)
                            throw new InvalidValueException(arg, opt.Name());
                        var fmtThe =
                        $"{{0,{widthThe}{CountComma.Func(true)}}} ";
                        opt.invoke = (it) =>
                        {
                            return string.Format(fmtThe, it);
                        };
                    }
                    else
                    {
                        throw new InvalidValueException(arg, opt.Name());
                    }
                });

        static public readonly IFunc<DateTime, string> DateFormat =
            new Function<DateTime, string>("--date-format=",
                help: "FORMAT",
                invoke: (it) => $"{it:yyyy-MM-dd HH:mm:ss} ",
                parse: (opt, arg) =>
                {
                    var formatThe = $"{arg} ";
                    opt.invoke = (it) => it.ToString(formatThe);
                });

        static public readonly IParser[] Parsers = new IParser[]
        {
            (IParser) CaseOpt,
            (IParser) GetFileDate,
            (IParser) MakeRelativePath,
            (IParser) CountComma,
            (IParser) ExclFilenameFilter,
            (IParser) ExclDirnameFilter,
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
        };

        static public readonly IParser[] ConfigParsers = new IParser[]
        {
            (IParser) CaseOpt,
            (IParser) CountComma,
            (IParser) HiddenFilter,
            (IParser) SizeFormat,
            (IParser) CountFormat,
            (IParser) DateFormat,
            SortOpt,
        };

        static public readonly IParser[] ConfigParsers2 = new IParser[]
        {
            (IParser) ExclFilenameFilter,
            (IParser) ExclDirnameFilter,
        };
    }
}
