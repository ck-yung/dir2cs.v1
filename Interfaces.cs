namespace dir2
{
    interface IParser
    {
        string Name();
        void Parse(string[] args);
    }

    interface IFunc<T, R>
    {
        R Func(T arg);
    }
}
