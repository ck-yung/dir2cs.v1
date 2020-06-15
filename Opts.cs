using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dir2
{
    static partial class Opts
    {
        static public readonly IFunc<long, bool> MaxFileSizeFilter =
            new Function<long, bool>("--size-within=",
                help: "NUMBER",
                invoke: (_) => true,
                parse: (opt, args) =>
                {
                    if (int.TryParse(args[0],
                        out int intTemp))
                    {
                        opt.invoke = (it) => intTemp > it;
                    }
                    else throw new InvalidValueException(
                        args[0], opt.Name());
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

        static public readonly IFunc<IEnumerable<InfoFile>, InfoSum> SumBy =
            new Function<IEnumerable<InfoFile>, InfoSum>(
                "--sum=", help: "ext|dir",
                invoke: (seqThe) => seqThe
                .Select((it) =>
                {
                    Console.Write(ItemText(it.ToString()));
                    return it;
                })
                .Aggregate(new InfoSum(InfoFile.BaseDir),
                (acc, it) => acc.AddWith(it)),
                parse: (opt, args) =>
                {
                    switch (args[0])
                    {
                        case "ext":
                            opt.invoke = (seqThe) => seqThe
                            .GroupBy((it) => Path.GetExtension(it.Filename))
                            .Select((grp) => grp.Aggregate(new InfoSum(
                                string.IsNullOrEmpty(grp.Key)
                                ? "*no-ext*" : grp.Key),
                            (acc, it) => acc.AddWith(it)))
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

        static public readonly IFunc<string, IEnumerable<string>> GetFiles =
            new Function<string, IEnumerable<string>>(
                "--dir=", help: "TODO",
                invoke: (dirname) => Helper.GetFiles(dirname),
                parse: (opt, args) => { });

        static public readonly IParser[] Parsers = new IParser[]
        {
            (IParser) GetFileDate,
            (IParser) MakeRelativePath,
            (IParser) MaxFileSizeFilter,
            TotalOpt,
            HideOpt,
            (IParser) GetFiles,
            (IParser) SumBy,
        };
    }
}
