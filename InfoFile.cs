using System;
using System.IO;
using System.Text;

namespace dir2
{
    class InfoSum
    {
        public int Count { get; private set; }
        public long Length { get; private set; }
        public DateTime DateTime { get; private set; }
        = DateTime.MaxValue;
        public DateTime Last { get; private set; }
        = DateTime.MinValue;

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append($"{Length,7} ");
            buf.Append($"{DateTime:yyyy-MM-dd HH:mm:ss} ");
            buf.Append("- ");
            buf.Append($"{Last:yyyy-MM-dd HH:mm:ss} ");
            buf.Append($"{Count,4}");
            return buf.ToString();
        }

        public InfoSum AddWith(FileInfo arg)
        {
            Count += 1;
            Length += arg.Length;
            if (DateTime > arg.LastWriteTime) DateTime = arg.LastWriteTime;
            if (Last < arg.LastWriteTime) Last = arg.LastWriteTime;
            return this;
        }
    }
}