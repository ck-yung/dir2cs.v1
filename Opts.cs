using System;
using System.IO;

namespace dir2
{
    static partial class Opts
    {
        static public readonly IFunc<long, bool> MaxFileSizeFilter =
            new Function<long, bool>("--size-within=",
                invoke: (_) => true,
                parse: (opt, args) =>
                {
                    if (args.Length != 1) return;
                    if (int.TryParse(args[0],
                        out int intTemp))
                    {
                        opt.invoke = (it) => intTemp > it;
                    }
                });

        static public Func<string, string> ItemText
        { get; private set; } = (it) => $"{it}{Environment.NewLine}";
        static public Func<string, string> TotalText
        { get; private set; } = (it) => $"{it}{Environment.NewLine}";
        static public readonly IParser TotalOpt = new Parser("--total=",
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
                        throw new ArgumentException(
                            $"'{args[0]}' is unknwon to {opt.Name()}");
                }
            });

        static public Func<string, string> SizeText
        { get; private set; } = (it) => it;
        static public Func<string, string> DateText
        { get; private set; } = (it) => it;
        static public Func<string, string> CountText
        { get; private set; } = (it) => it;
        static public readonly IParser HideOpt = new Parser("--hide=",
            requireUnique: false,
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
                            throw new ArgumentException(
                                $"'{args[0]}' is unknwon to {opt.Name()}");
                    }
                }
            });

        static public readonly IFunc<FileInfo,DateTime> GetFileDate =
            new Switcher<FileInfo, DateTime>("--create-date",
            invoke: (it) => it.LastWriteTime, alt: (it) => it.CreationTime);

        static public readonly IParser[] Parsers = new IParser[]
        {
            (IParser) MaxFileSizeFilter,
            TotalOpt,
            HideOpt,
            (IParser) GetFileDate,
        };
    }
}
