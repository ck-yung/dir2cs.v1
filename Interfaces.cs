using System.Collections.Generic;

namespace dir2
{
    interface IParser
    {
        string Name();
        IEnumerable<string> Parse(IEnumerable<string> args);
    }

    interface IFunc<T, R>
    {
        R Func(T arg);
    }
}
