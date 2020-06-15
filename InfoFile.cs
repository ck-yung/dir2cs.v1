using System;
using System.IO;
using System.Text;

namespace dir2
{
    class InfoFile
    {
        public string FullName { get; private set; }
        public string Filename { get; private set; }
        public long Length { get; private set; }
        public DateTime DateTime { get; private set; }
        private InfoFile() { }

        static InfoFile None = new InfoFile();

        static public InfoFile From(string filename)
        {
            try
            {
                var info = new FileInfo(filename);
                var rtn = new InfoFile();
                rtn.Filename = Path.GetFileName(filename);
                rtn.FullName = info.FullName;
                rtn.Length = info.Length;
                rtn.DateTime = Opts.GetFileDate.Func(info);
                return rtn;
            }
            catch
            {
                return None;
            }
        }
        public bool IsNotNone { get => !ReferenceEquals(this, None); }
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append(Opts.SizeText($"{Length,7} "));
            buf.Append(Opts.DateText($"{DateTime:yyyy-MM-dd HH:mm:ss} "));
            buf.Append(FullName);
            return buf.ToString();
        }
    }

    class InfoSum
    {
        public readonly string Name;
        public InfoSum(string name)
        {
            Name = name;
        }

        public int Count { get; private set; }
        public int AddCount { get; private set; }
        public long Length { get; private set; }
        public DateTime DateTime { get; private set; }
        = DateTime.MaxValue;
        public DateTime Last { get; private set; }
        = DateTime.MinValue;

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append(Opts.SizeText($"{Length,7} "));
            buf.Append(Opts.DateText($"{DateTime:yyyy-MM-dd HH:mm:ss} "));
            buf.Append(Opts.DateText("- "));
            buf.Append(Opts.DateText($"{Last:yyyy-MM-dd HH:mm:ss} "));
            buf.Append(Opts.CountText($"{Count,4} "));
            buf.Append(Name);
            return buf.ToString();
        }

        public InfoSum AddWith(InfoFile arg)
        {
            Count += 1; AddCount += 1;
            Length += arg.Length;
            if (DateTime > arg.DateTime) DateTime = arg.DateTime;
            if (Last < arg.DateTime) Last = arg.DateTime;
            return this;
        }

        public InfoSum AddWith(InfoSum arg)
        {
            Count += arg.Count; AddCount += 1;
            Length += arg.Length;
            if (DateTime > arg.DateTime) DateTime = arg.DateTime;
            if (Last < arg.Last) Last = arg.Last;
            return this;
        }
    }
}