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
                    if (int.TryParse(args[0].Substring(opt.Name().Length),
                        out int intTemp))
                    {
                        opt.invoke = (it) => intTemp > it;
                    }
                });
    }
}