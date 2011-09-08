using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NntpClient.Decoders {
    internal abstract class BinaryDecoder : IBinaryDecoder {
        string peekLine;

        public BinaryDecoder(StreamReader reader) {
            Reader = reader;
            // some messages have multiple blank lines between the header & message body
            while(PeekLine() == string.Empty)
                ReadLine();
        }
        protected string ReadLine() {
            string line = peekLine ?? Reader.ReadLine();
            peekLine = null;
            return line;
        }

        protected string PeekLine() {
            return peekLine ?? (peekLine = Reader.ReadLine());
        }

        protected string GetCrc32() {
            Crc32 crc = new Crc32();
            string crcHash = string.Empty;
            crcHash = crc.ComputeHash(Result).Aggregate(crcHash, (a, c) => a += c.ToString("x2"));
            Result.Position = 0;
            return crcHash;
        }

        public abstract void Decode(Action<IBinaryDecoder> OnChunkDownloaded);

        protected StreamReader Reader { get; private set; }

        public abstract MemoryStream Result { 
            get; 
        }
        public abstract string ExpectedCrc32 { 
            get; 
        }
        public abstract string ActualCrc32 {
            get;
        }
        public int BytesRead {
            get {
                if(Result == null)
                    return -1;
                return (int)Result.Length;
            }
        }

        public abstract int ByteOffset {
            get;
        }

        public abstract string Filename {
            get;
        }

        public abstract int Size {
            get;
        }

        public abstract int Part {
            get;
        }

        public abstract int TotalParts {
            get;
        }
    }
}
