using System;
using System.IO;
using System.Text;

namespace dir2
{
    class InfoFile
    {
        public string FullName { get; init; }
        public string Filename { get; init; }
        public long Length { get; init; }
        public DateTime DateTime { get; init; }
        public FileAttributes Attributes { get; init; }

        public bool IsHidden
        {
            get
            {
                if (Attributes.HasFlag(FileAttributes.Hidden)) return true;
                if (string.IsNullOrEmpty(FullName)) return true;
                if (Path.GetFileName(FullName)[0] == '.') return true;
                return false;
            }
        }

        private InfoFile()
        {
            FullName = "||";
            Filename = "|";
        }

        static InfoFile None = new InfoFile();

        private InfoFile(string filename)
        {
            var infoFile = new FileInfo(filename);
            Filename = Path.GetFileName(filename);
            FullName = infoFile.FullName;
            Length = infoFile.Length;
            DateTime = Opts.GetFileDate.Func(infoFile);
            Attributes = infoFile.Attributes;
        }

        static public InfoFile From(string filename)
        {
            try
            {
                return new InfoFile(filename);
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
            buf.Append(Opts.SizeText(Opts.SizeFormat.Func(Length)));
            buf.Append(Opts.DateText(Opts.DateFormat.Func(DateTime)));
            buf.Append(RelativePath(FullName));
            return buf.ToString();
        }

        static public Func<string, string> RelativePath
        { get; private set; } = (it) => it;
        static public string BaseDir { get; private set; } = "?";
        static public void InitDir(string dirname)
        {
            BaseDir = dirname;
            RelativePath = Opts.MakeRelativePath.Func(dirname);
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
            buf.Append(Opts.SizeText(Opts.SizeFormat.Func(Length)));
            buf.Append(Opts.DateText(Opts.DateFormat.Func(DateTime)));
            buf.Append(Opts.DateText("- "));
            buf.Append(Opts.DateText(Opts.DateFormat.Func(Last)));
            buf.Append(Opts.CountText(Opts.CountFormat.Func(Count)));
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