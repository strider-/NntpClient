using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;

namespace NntpClient {
    internal class Connection : IDisposable {
        string peekLine;
        StreamReader r;
        StreamWriter w;

        public Connection(string hostname, int port, bool useSsl) {
            TcpClient c = new TcpClient();
            c.Connect(hostname, port);
            Stream stream = c.GetStream();

            if(useSsl) {
                SslStream sslStream = new SslStream(c.GetStream(), false);
                sslStream.AuthenticateAsClient(hostname);
                stream = sslStream;
            }

            this.Encoding = Encoding.GetEncoding(1252);
            r = new StreamReader(stream, this.Encoding);
            w = new StreamWriter(stream, this.Encoding);
            w.AutoFlush = true;
        }

        public void Dispose() {
            r.Close();
            w.Close();
        }

        public string ReadLine() {
            string line = peekLine ?? r.ReadLine();
            peekLine = null;
            return line;
        }

        public string PeekLine() {
            return peekLine ?? (peekLine = r.ReadLine());
        }

        public ServerReply WriteLine(string line, params object[] args) {
            w.WriteLine(line, args);
            string result = ReadLine();
            var reply = ServerReply.Parse(result);
            return reply;
        }

        public Encoding Encoding { get; private set; }
    }
}
