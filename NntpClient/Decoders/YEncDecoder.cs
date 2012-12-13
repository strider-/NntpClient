using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace NntpClient.Decoders {
    internal class YEncDecoder : BinaryDecoder {
        // specify RightToLeft for regex options
        const string PATTERN_YENC_HEADER = @"(?<key>[A-z0-9]+)=(?<value>.*?)(?:\s|$)";

        Dictionary<string, string> meta;
        MemoryStream destination;
        string expectedCrc32, actualCrc32;

        public YEncDecoder(Connection conn)
            : base(conn) {
                destination = new MemoryStream();
                ReadHeader();
        }

        private void ReadHeader() {
            string ybegin = string.Empty, ypart = string.Empty;
            List<Dictionary<string, string>> dicts = new List<Dictionary<string, string>>();

            ybegin = Connection.ReadLine();
            dicts.Add(ParseYEncKeywordLine(ybegin));

            if(Connection.PeekLine().StartsWith("=ypart")) {
                ypart = Connection.ReadLine();
                dicts.Add(ParseYEncKeywordLine(ypart));
            }

            meta = dicts.SelectMany(d => d).ToDictionary(k => k.Key, v => v.Value);
        }
        private Dictionary<string, string> ParseYEncKeywordLine(string header) {
            var mc = Regex.Matches(header, PATTERN_YENC_HEADER, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.RightToLeft);
            return mc.OfType<Match>().ToDictionary(k => k.Groups["key"].Value, v => v.Groups["value"].Value);
        }
        private T GetValue<T>(string key) {
            if(meta.ContainsKey(key))
                return (T)Convert.ChangeType(meta[key], typeof(T));
            return default(T);
        }
        private int GetSize() {
            int begin = GetValue<int>("begin");
            int end = GetValue<int>("end");

            if(begin > 0 && end > 0)
                return (end - begin) + 1;

            return 0;
        }

        public override void Decode(Action<IBinaryDecoder> OnChunkDownloaded) {                        
            string line;
            while(!(line = Connection.ReadLine()).StartsWith("=yend")) {
                byte[] raw = Connection.Encoding.GetBytes(line);
                byte[] decoded = new byte[line.Length];
                int length = 0;

                for(int i = (raw[0] == 0x2e && raw[1] == 0x2e) ? 1 : 0; i < raw.Length; i++) {
                    if(raw[i] == '=') {
                        i++;
                        decoded[length++] = (byte)((raw[i] - 0x40) - 0x2a);
                    } else {
                        decoded[length++] = (byte)(raw[i] - 0x2a);
                    }
                }
                destination.Write(decoded, 0, length);
                OnChunkDownloaded(this);
            }
            destination.Position = 0;

            var yFooterDict = ParseYEncKeywordLine(line);
            var keys = new string[] { "pcrc32", "crc32", "crc" };
            var crcKey = yFooterDict.First(kvp => yFooterDict.ContainsKey(kvp.Key)).Key;
            
            actualCrc32 = GetCrc32();
            expectedCrc32 = yFooterDict[crcKey];

            if(!string.IsNullOrWhiteSpace(expectedCrc32))
                expectedCrc32 = expectedCrc32.ToLower();

            Connection.ReadLine();
        }

        public override MemoryStream Result {
            get { return destination; }
        }

        public override int ByteOffset {
            get { return GetValue<int>("begin") - 1; }
        }

        public override string Filename {
            get { return meta["name"]; }
        }

        public override int Size {
            get { return GetSize(); }
        }

        public override int Part {
            get { return GetValue<int>("part"); }
        }

        public override int TotalParts {
            get { return GetValue<int>("total"); }
        }

        public override string ExpectedCrc32 {
            get { return expectedCrc32; }
        }

        public override string ActualCrc32 { 
            get { return actualCrc32; } 
        }
    }
}
