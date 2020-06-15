using System;

namespace dir2
{
    static partial class Opts
    {
        private class Function<T, R> : IFunc<T, R>, IParser
        {
            public Func<T,R> invoke { get; set; }
            public R Func(T arg)
            {
                return invoke(arg);
            }

            public Function(string name,
                Func<T,R> invoke,
                Action<Function<T, R>, string[]> parse)
            {
                this.name = name;
                this.invoke = invoke;
                this.parse = parse;
            }

            readonly string name;
            readonly Action<Function<T, R>, string[]> parse;

            public string Name()
            {
                return name;
            }

            public void Parse(string[] args)
            {
                parse(this, args);
            }
        }
    }
}