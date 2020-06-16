using System;
using System.Linq;

namespace dir2
{
    static partial class Opts
    {
        private abstract class AbstractParser<T, R> : IParser, IFunc<T, R>
        {
            protected readonly string name;
            protected readonly string help;
            public Func<T, R> invoke { get; set; }

            public AbstractParser(string name,
                string help)
            {
                this.name = name;
                this.help = help;
            }

            public string Name()
            {
                return name;
            }

            public abstract string[] Parse(string[] args);

            public virtual R Func(T arg)
            {
                return invoke(arg);
            }

            public override string ToString()
            {
                return $"{name,19}{help}";
            }
        }

        private class Function<T, R> : AbstractParser<T, R>
        {
            protected readonly Action<Function<T, R>, string> parse;

            public Function(string name, Func<T, R> invoke,
                Action<Function<T, R>, string> parse, string help = "")
                : base(name, help)
            {
                this.invoke = invoke;
                this.parse = parse;
            }

            public override string[] Parse(string[] args)
            {
                var found = args
                    .GroupBy((it) => it.StartsWith(name))
                    .ToDictionary((grp) => grp.Key, (grp) => grp);

                if (found.ContainsKey(true))
                {
                    var values = found[true]
                        .Select((it) => it.Substring(name.Length))
                        .Where((it) => !string.IsNullOrEmpty(it))
                        .Distinct()
                        .ToArray();

                    if (values.Length > 1)
                        throw new TooManyValuesException(name);

                    if (values.Length > 0) parse(this, values[0]);
                }
                else return args;

                return found.ContainsKey(false)
                    ? found[false].ToArray()
                    : Helper.emptyStrings;
            }
        }

        private class Switcher<T,R>: AbstractParser<T,R>
        {
            protected readonly Func<T, R> alt;
            protected readonly Action<Switcher<T, R>> postAlt;

            public Switcher(string name,
                Func<T, R> invoke, Func<T, R> alt,
                string help = "",
                Action<Switcher<T, R>> postAlt = null)
                :base(name,help)
            {
                this.invoke = invoke;
                this.alt = alt;
                this.postAlt = postAlt;
            }

            public override string[] Parse(string[] args)
            {
                var found = args
                    .GroupBy((it) => it == name)
                    .ToDictionary((grp) => grp.Key, (grp) => grp);

                if (found.ContainsKey(true))
                {
                    invoke = alt;
                    postAlt?.Invoke(this);
                }
                else return args;

                return found.ContainsKey(false)
                    ? found[false].ToArray()
                    : Helper.emptyStrings;
            }

            public override string ToString()
            {
                return string.IsNullOrEmpty(help)
                    ? $"{name,18}" : $"{name,18} \t{help}";
            }
        }

        private class Parser: AbstractParser<bool,bool>
        {
            protected readonly Action<Parser, string> parse;

            public Parser(string name, Action<Parser, string> parse,
                string help = ""): base(name, help)
            {
                this.parse = parse;
            }

            public override string[] Parse(string[] args)
            {
                var found = args
                    .GroupBy((it) => it.StartsWith(name))
                    .ToDictionary((grp) => grp.Key, (grp) => grp);

                if (found.ContainsKey(true))
                {
                    var values = found[true]
                        .Select((it) => it.Substring(name.Length))
                        .Where((it) => !string.IsNullOrEmpty(it))
                        .Distinct()
                        .ToArray();

                    if (values.Length > 1)
                        throw new TooManyValuesException(name);

                    if (values.Length > 0) parse(this, values[0]);
                }
                else return args;

                return found.ContainsKey(false)
                    ? found[false].ToArray()
                    : Helper.emptyStrings;
            }

            public override bool Func(bool arg)
            {
                return false;
            }
        }

        private class Function2<T, R> : AbstractParser<T, R>
        {
            protected readonly Action<Function2<T, R>, string[]> parse;

            public Function2(string name, Func<T, R> invoke,
                Action<Function2<T, R>, string[]> parse, string help = "")
                : base(name, help)
            {
                this.invoke = invoke;
                this.parse = parse;
            }

            public override string[] Parse(string[] args)
            {
                var found = args
                    .GroupBy((it) => it.StartsWith(name))
                    .ToDictionary((grp) => grp.Key, (grp) => grp);

                if (found.ContainsKey(true))
                {
                    var values = found[true]
                        .Select((it) => it.Substring(name.Length))
                        .Select((it) => it.Split(','))
                        .SelectMany((it) => it)
                        .Where((it) => !string.IsNullOrEmpty(it))
                        .Distinct()
                        .ToArray();

                    if (values.Length > 0) parse(this, values);
                }
                else return args;

                return found.ContainsKey(false)
                    ? found[false].ToArray()
                    : Helper.emptyStrings;
            }
        }
    }
}
