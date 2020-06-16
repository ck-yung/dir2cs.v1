using System;
using System.Linq;

namespace dir2
{
    static partial class Opts
    {
        private class Function777<T, R> : IFunc<T, R>, IParser
        {
            public Func<T,R> invoke { get; set; }
            public R Func(T arg)
            {
                return invoke(arg);
            }

            readonly bool requireUnique;
            public Function777(string name,
                Func<T,R> invoke,
                Action<Function777<T, R>, string[]> parse,
                string help = "", bool requireUnique = true)
            {
                this.name = name;
                this.invoke = invoke;
                this.parse = parse;
                this.help = help;
                this.requireUnique = requireUnique;
            }

            readonly string name;
            readonly string help;
            readonly Action<Function777<T, R>, string[]> parse;

            public string Name()
            {
                return name;
            }

            public string[] Parse(string[] args)
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

                    if (requireUnique && (values.Length > 1))
                        throw new TooManyValuesException(name);

                    if (values.Length > 0) parse(this, values);
                }
                else return args;

                return found.ContainsKey(false)
                    ? found[false].ToArray()
                    : Helper.emptyStrings;
            }

            public override string ToString()
            {
                return $"{name,19}{help}";
            }
        }

        private class Parser777 : IParser
        {
            readonly string name;
            readonly string help;
            public string Name()
            {
                return name;
            }

            readonly Action<Parser777, string[]> parse;
            public string[] Parse(string[] args)
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

                    if (requireUnique && (values.Length > 1))
                        throw new TooManyValuesException(name);

                    if (values.Length > 0) parse(this, values);
                }
                else return args;

                return found.ContainsKey(false)
                    ? found[false].ToArray()
                    : Helper.emptyStrings;
            }

            readonly bool requireUnique;
            public Parser777(string name,
                Action<Parser777, string[]> parse,
                string help = "", bool requireUnique = true)
            {
                this.name = name;
                this.help = help;
                this.parse = parse;
                this.requireUnique = requireUnique;
            }
            public override string ToString()
            {
                return $"{name,19}{help}";
            }
        }

        private class Switcher777<T, R> : IFunc<T, R>, IParser
        {
            Func<T, R> invoke { get; set; }
            readonly Func<T, R> alt;
            public R Func(T arg)
            {
                return invoke(arg);
            }

            readonly string name;
            readonly string help;
            public string Name()
            {
                return name;
            }

            readonly Action<Switcher777<T, R>> postAlt;
            public string[] Parse(string[] args)
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

            public Switcher777(string name,
                Func<T,R> invoke, Func<T,R> alt,
                string help = "",
                Action<Switcher777<T,R>> postAlt = null)
            {
                this.name = name;
                this.help = help;
                this.invoke = invoke;
                this.alt = alt;
                this.postAlt = postAlt;
            }

            public override string ToString()
            {
                return $"{name,18} {help}";
            }
        }
    }
}
