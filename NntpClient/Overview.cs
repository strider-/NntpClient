using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient
{
    public class Overview
    {
        private string[] _fields;

        internal Overview(string raw)
        {
            _fields = raw.Split('\t');
        }

        public string ArticleId { get { return _fields[0]; } }
        public string Subject { get { return _fields[1]; } }
        public string Author { get { return _fields[2]; } }
        public DateTime Date { get { return DateTime.Parse(_fields[3]).ToUniversalTime(); } }
        public string MessageId { get { return _fields[4]; } }
        public string References { get { return _fields[5]; } }
        public uint ByteCount { get { return UInt32.Parse(_fields[6]); } }
        public uint LineCount { get { return UInt32.Parse(_fields[7]); } }
        public string[] AdditionalFields
        {
            get
            {
                if(_fields.Length == 8)
                {
                    return Enumerable.Empty<string>().ToArray();
                }
                else
                {
                    return _fields.Skip(8).ToArray();
                }
            }
        }
    }
}
