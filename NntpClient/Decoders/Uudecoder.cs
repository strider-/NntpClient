using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NntpClient.Decoders {
    internal class Uudecoder : BinaryDecoder {
        MemoryStream destination;
        string crc32, name;

        public Uudecoder(Connection conn)
            : base(conn) {
            destination = new MemoryStream();
            string header = Connection.ReadLine();
            name = header.Substring(10);
        }

        public override void Decode(Action<IBinaryDecoder> OnChunkDownloaded) {
            string line;
            while((line = Connection.ReadLine()) != ".") {
                if(line == "`" || line == "end" || string.IsNullOrWhiteSpace(line))
                    continue;
                byte[] raw = Connection.Encoding.GetBytes(line);
                int length = raw[0] - 32;
                int pos = 1, written = 0;

                while(written != length) {
                    int toRead = Math.Min(length - written, 3) + 1;

                    byte[] block = new byte[toRead];
                    for(int i = 0; i < toRead; i++)
                        block[i] = (byte)(raw[pos++] - 32);

                    byte[] decoded = new byte[toRead - 1];
                    for(int i = 0, e = 2; i < decoded.Length; i++, e += 2) {
                        decoded[i] = (byte)((block[i] << e) & 0xFF | (block[i + 1] >> (6 - e)) & (byte)(Math.Pow(2, e) - 1));
                    }
                    written += decoded.Length;
                    destination.Write(decoded, 0, decoded.Length);
                    OnChunkDownloaded(this);
                }
            }
            destination.Position = 0;
            crc32 = GetCrc32();
        }

        public override MemoryStream Result {
            get { return destination; }
        }

        public override string ExpectedCrc32 {
            get { return null; }
        }

        public override string ActualCrc32 {
            get { return crc32; }
        }

        public override int ByteOffset {
            get { return -1; }
        }

        public override string Filename {
            get { return name; }
        }

        public override int Size {
            get { return 0; }
        }

        public override int Part {
            get { return 0; }
        }

        public override int TotalParts {
            get { return 0; }
        }
    }
}
