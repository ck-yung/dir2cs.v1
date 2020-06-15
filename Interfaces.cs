namespace dir2
{
    interface IParser
    {
        string Name();
        string[] Parse(string[] args);
    }

    interface IFunc<T, R>
    {
        R Func(T arg);
    }
}
