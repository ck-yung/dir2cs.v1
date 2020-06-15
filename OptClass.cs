using System;
using System.Linq;

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

            readonly bool requireUnique;
            public Function(string name,
                Func<T,R> invoke,
                Action<Function<T, R>, string[]> parse,
                bool requireUnique = true)
            {
                this.name = name;
                this.invoke = invoke;
                this.parse = parse;
                this.requireUnique = requireUnique;
            }

            readonly string name;
            readonly Action<Function<T, R>, string[]> parse;

            public string Name()
            {
                return name;
            }

            public void Parse(string[] args)
            {
                var values = args
                    .Where((it) => it.StartsWith(name))
                    .Select((it) => it.Substring(name.Length))
                    .Select((it) => it.Split(','))
                    .SelectMany((it) => it)
                    .Distinct()
                    .ToArray();

                if (requireUnique && (values.Length>1))
                    throw new ArgumentException(
                            $"Too many option to {name}");

                if (values.Length>0) parse(this, values);
            }
        }

        private class Parser : IParser
        {
            readonly string name;
            public string Name()
            {
                return name;
            }

            readonly Action<Parser, string[]> parse;
            public void Parse(string[] args)
            {
                var values = args
                    .Where((it) => it.StartsWith(name))
                    .Select((it) => it.Substring(name.Length))
                    .Select((it) => it.Split(','))
                    .SelectMany((it) => it)
                    .Distinct()
                    .ToArray();

                if (requireUnique && (values.Length > 1))
                    throw new ArgumentException(
                            $"Too many option to {name}");

                if (values.Length > 0) parse(this, values);
            }

            readonly bool requireUnique;
            public Parser(string name,
                Action<Parser, string[]> parse,
                bool requireUnique = true)
            {
                this.name = name;
                this.parse = parse;
                this.requireUnique = requireUnique;
            }
        }

        private class Switcher<T, R> : IFunc<T, R>, IParser
        {
            Func<T, R> invoke { get; set; }
            readonly Func<T, R> alt;
            public R Func(T arg)
            {
                return invoke(arg);
            }

            readonly string name;
            public string Name()
            {
                return name;
            }

            readonly Action<Switcher<T, R>> postAlt;
            public void Parse(string[] args)
            {
                if (args
                    .Where((it) => it==name)
                    .Any())
                {
                    invoke = alt;
                    postAlt?.Invoke(this);
                }
            }

            public Switcher(string name,
                Func<T,R> invoke, Func<T,R> alt,
                Action<Switcher<T,R>> postAlt = null)
            {
                this.name = name;
                this.invoke = invoke;
                this.alt = alt;
                this.postAlt = postAlt;
            }
        }
    }
}
