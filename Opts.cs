namespace dir2
{
    static partial class Opts
    {
        static public IFunc<long, bool> MaxFileSizeFilter =
            new Function<long, bool>("--size-within=",
                invoke: (_) => true,
                parse: (args) =>
                {
                    if (args.Length != 1) return;
                    if (int.TryParse(args[0].Substring(this.name.Length),
                        out int intTemp))
                    {
                        this.Invoke = (it) => intTemp > it.Length;
                    }
                });
    }
}
