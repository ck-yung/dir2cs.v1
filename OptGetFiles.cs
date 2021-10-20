using System.Text.RegularExpressions;

namespace dir2
{
    static partial class Opts
    {

        static Action<string> PrintDir { get; set; } =
            (dirname) => Helper.PrintDir(dirname, (_) => true);

        static public readonly IFunc<string, IEnumerable<string>> GetFiles =
            new Function<string, IEnumerable<string>>(
                "--dir=", help: "sub|off|only|tree",
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
                            opt._Invoke = (dirname) => Helper.GetAllFiles(dirname);
                            break;
                        case "off":
                            PrintDir = (_) => { };
                            break;
                        case "only":
                            opt._Invoke = (dirname) =>
                            {
                                Helper.PrintDir(dirname,
                                    (it) => FilenameFilter!.Func(it));
                                TotalText = (_) => "";
                                return Helper.emptyStrings;
                            };
                            break;
                        case "tree":
                            opt._Invoke = (dirname) =>
                            {
                                Helper.PrintTree(dirname);
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
                    opt._Invoke = arg switch
                    {
                        "long" => (it) => $"{it,19:N0} ",
                        "short" => (it) => $"{Helper.ToKiloUnit(it)} ",
                        _ => throw new InvalidValueException(
                            arg, opt.Name()),
                    };
                });

        static public readonly IFunc<bool, string> CountComma =
            new Switcher<bool, string>("--count-comma",
                invoke: (_) => "", alt: (_) => ":N0",
                postAlt: (opt) =>
                {
                    ((IParser)(CountFormat!)).Parse(
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
                        opt._Invoke = (it) =>
                        {
                            return string.Format(fmtThe, it);
                        };
                    }
                    else
                    {
                        throw new InvalidValueException(arg, opt.Name());
                    }
                });

        static public readonly IFunc<string, string> ToRegexText =
            new Switcher<string, string>("--regex",
                help: "enclosed by quotion mark",
                invoke: (it) =>
                {
                    var regText = new System.Text.StringBuilder("^");
                    regText.Append(it
                        .Replace(@"\", @"\\")
                        .Replace("^", @"\^")
                        .Replace("$", @"\$")
                        .Replace(".", @"\.")
                        .Replace("?", ".")
                        .Replace("*", ".*")
                        .Replace("(", @"\(")
                        .Replace(")", @"\)")
                        .Replace("[", @"\[")
                        .Replace("]", @"\]")
                        .Replace("{", @"\{")
                        .Replace("}", @"\}")
                        ).Append('$');
                    return regText.ToString();
                }, alt: (it) => it);

        static public readonly IFunc<DateTime, string> DateFormat =
            new Function<DateTime, string>("--date-format=",
                help: "FORMAT",
                invoke: (it) => $"{it:yyyy-MM-dd HH:mm:ss} ",
                parse: (opt, arg) =>
                {
                    var formatThe = $"{arg} ";
                    opt._Invoke = (it) => it.ToString(formatThe);
                });

        static public readonly IFunc<bool, bool> EncodeConsoleOuput =
            new Switcher<bool, bool>("--utf8", help: "see --help=utf8",
                invoke: (_) => { return false; }, alt: (_) =>
                {
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                    return true;
                });

        static Func<IEnumerable<InfoSum>, IEnumerable<InfoSum>>
            SumOrder = (seqThe) => seqThe;

        static public readonly IFunc<IEnumerable<InfoFile>,
            IEnumerable<InfoFile>> OrderOpt =
            new Switcher<IEnumerable<InfoFile>, IEnumerable<InfoFile>>(
                "--reverse",
                invoke: (seqThe) => seqThe,
                alt: (seqThe) => seqThe.Reverse(),
                postAlt: (_) =>
                {
                    SumOrder = (seqThe) => seqThe.Reverse();
                });
    }
}

