using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dir2
{
    static partial class Opts
    {
        static public readonly IFunc<long, bool> MinFileSizeFilter =
            new Function<long, bool>("--size-beyond=",
                help: "NUMBER",
                invoke: (_) => true,
                parse: (opt, args) =>
                {
                    if (Helper.TryParseAsLong(args[0],
                        out long longThe) && (longThe >= 0))
                    {
                        opt.invoke = (it) => it >= longThe;
                    }
                    else throw new InvalidValueException(
                        args[0], opt.Name());
                });

        static public readonly IFunc<long, bool> MaxFileSizeFilter =
            new Function<long, bool>("--size-within=",
                help: "NUMBER",
                invoke: (_) => true,
                parse: (opt, args) =>
                {
                    if (Helper.TryParseAsLong(args[0],
                        out long longThe) && (longThe >= 0))
                    {
                        opt.invoke = (it) => longThe > it;
                    }
                    else throw new InvalidValueException(
                        args[0], opt.Name());
                });

        static public readonly IFunc<DateTime, bool> MinFileDateFilter =
            new Function<DateTime, bool>("--date-beyond=", help: "DATETIME",
                invoke: (_) => true, parse: (opt, args) =>
                {
                    if (Helper.TryParseDateTime(args[0], out DateTime result))
                    {
                        opt.invoke = (it) => result > it;
                    }
                    else throw new InvalidValueException(
                        args[0], opt.Name());
                });

        static public readonly IFunc<DateTime, bool> MaxFileDateFilter =
            new Function<DateTime, bool>("--date-within=", help: "DATETIME",
                invoke: (_) => true, parse: (opt, args) =>
                {
                    if (Helper.TryParseDateTime(args[0], out DateTime result))
                    {
                        opt.invoke = (it) => it >= result;
                    }
                    else throw new InvalidValueException(
                        args[0], opt.Name());
                });

        static public readonly IFunc<string, bool> FileExtFilter =
            new Function<string, bool>("--no-ext=", help: "excl|only",
                invoke: (_) => true,
                parse: (opt, args) =>
                {
                    switch (args[0])
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
                            throw new InvalidValueException(
                                args[0], opt.Name());
                    }
                });

        static public Func<string, string> ItemText
        { get; private set; } = (it) => $"{it}{Environment.NewLine}";
        static public Func<string, string> TotalText
        { get; private set; } = (it) => $"{it}{Environment.NewLine}";
        static public readonly IParser TotalOpt = new Parser("--total=",
            help: "off|only",
            parse: (opt, args) =>
            {
                switch (args[0])
                {
                    case "off":
                        TotalText = (_) => "";
                        break;
                    case "only":
                        ItemText = (_) => "";
                        break;
                    default:
                        throw new InvalidValueException(
                            args[0], opt.Name());
                }
            });

        static public Func<string, string> SizeText
        { get; private set; } = (it) => it;
        static public Func<string, string> DateText
        { get; private set; } = (it) => it;
        static public Func<string, string> CountText
        { get; private set; } = (it) => it;
        static public readonly IParser HideOpt = new Parser("--hide=",
            help: "size,date,count", requireUnique: false,
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
                            throw new InvalidValueException(
                                args[0], opt.Name());
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
            parse: (opt, args) =>
            {
                switch (args[0])
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
                        throw new InvalidValueException(args[0], opt.Name());
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
                parse: (opt, args) =>
                {
                    PrintDir = (_) => { };
                    switch (args[0])
                    {
                        case "ext":
                            opt.invoke = (seqThe) => seqThe
                            .GroupBy((it) => Path.GetExtension(it.Filename))
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
                            throw new InvalidValueException(
                                args[0], opt.Name());
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
                parse: (opt, args) =>
                {
                    switch (args[0])
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
                            throw new InvalidValueException(args[0], opt.Name());
                    }
                });

        static public readonly IParser[] Parsers = new IParser[]
        {
            (IParser) GetFileDate,
            (IParser) MakeRelativePath,
            (IParser) MinFileSizeFilter,
            (IParser) MaxFileSizeFilter,
            (IParser) MinFileDateFilter,
            (IParser) MaxFileDateFilter,
            (IParser) FileExtFilter,
            TotalOpt,
            HideOpt,
            SortOpt,
            (IParser) GetFiles,
            (IParser) SumBy,
        };
    }
}
