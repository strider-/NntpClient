using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using NntpClient.EventArgs;

namespace NntpClient {
    /// <summary>
    /// Connects &amp; performs operations on a usenet server.
    /// </summary>
    public class Client : IDisposable {
        const string PATTERN_YENC_HEADER = @"part=(?<part>\d*)\sline=(?<line>\d*)\ssize=(?<size>\d*)\sname=(?<name>.*)";

        byte[] buffer;
        TcpClient client;
        StreamReader sr;
        StreamWriter sw;
        Encoding enc;
        
        /// <summary>
        /// Fired when an article could not be found on the server
        /// </summary>
        public event EventHandler<ArticleNotFoundEventArgs> ArticleNotFound = delegate { };

        /// <summary>
        /// Creates a new intance of the client.
        /// </summary>
        public Client() {
            client = new TcpClient();
            enc = Encoding.GetEncoding(1252);
        }
        /// <summary>
        /// Opens a connection to the given usenet server on the specified port, and whether or not to use ssl.
        /// </summary>
        /// <param name="hostname">Usenet hostname</param>
        /// <param name="port">Port to connect on</param>
        /// <param name="ssl">Uses a secure connection if true</param>
        public void Connect(string hostname, int port, bool ssl) {            
            client.Connect(hostname, port);
            Stream stream = client.GetStream();
            buffer = new byte[0x8000];
            
            if(ssl) {
                SslStream sslStream = new SslStream(client.GetStream(), true);
                sslStream.AuthenticateAsClient(hostname);
                stream = sslStream;
            }

            sr = new StreamReader(stream, enc);
            sw = new StreamWriter(stream, enc);
            sw.AutoFlush = true;

            var reply = ServerReply.Parse(ReadLine());
            Connected = reply.IsGood;
        }
        /// <summary>
        /// Closes the connection &amp; returns the statistics of the session.
        /// </summary>
        /// <returns></returns>
        public ConnectionResult Close() {
            if(Connected) {
                string msg = WriteLine("QUIT").Message;
                var result = ConnectionResult.Parse(msg);

                CleanUp();
                return result;
            }
            return null;
        }
        /// <summary>
        /// Closes the connection &amp; disposes of managed resources.
        /// </summary>
        public void Dispose() {
            Close();
        }
        /// <summary>
        /// Authenticates credentials against the current server.
        /// </summary>
        /// <param name="user">Usenet username</param>
        /// <param name="pass">Usenet password</param>
        /// <returns></returns>
        public void Authenticate(string user, string pass) {
            WriteLine("AUTHINFO USER {0}", user);            
            var result = WriteLine("AUTHINFO PASS {0}", pass);
            
            if(!result.IsGood) {
                throw new Exception(result.Message);
            }

            Authenticated = true;
            SetMode("READER");
        }
        /// <summary>
        /// Returns a collection of available usenet groups.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Group> GetGroups() {
            var groups = new List<Group>();
            string group;

            var reply = WriteLine("LIST");

            while((group = ReadLine()) != ".") {
                groups.Add(Group.Parse(group));
            }

            return groups;
        }
        /// <summary>
        /// Sets the current usenet group pointer.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public bool SetGroup(string groupName) {
            var result = WriteLine("GROUP {0}", groupName);

            if(result.IsGood) {
                CurrentGroup = groupName;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Sets the current usenet group pointer.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool SetGroup(Group group) {
            return SetGroup(group.Name);
        }
        /// <summary>
        /// Fetches only the headers of an article with the given ID.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetHeaders(string articleId) {
            var dict = new Dictionary<string, string>();
            var result = WriteLine("HEAD <{0}>", articleId.Trim('<', '>'));

            if(result.IsGood) {
                return ReadHeader();
            }
            
            ArticleNotFound(this, new ArticleNotFoundEventArgs(result, articleId));
            return null;
        }
        /// <summary>
        /// Downloads an article from usenet with the given ID. Returns null if the article could not be found.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public Article GetArticle(string articleId) {
            var result = WriteLine("ARTICLE <{0}>", articleId.Trim('<', '>'));
            string line = null;

            if(result.IsGood) {
                var dict = ReadHeader();

                string yEncHeader = string.Empty;
                while((yEncHeader = ReadLine()) == string.Empty)
                    ;

                var m = Regex.Match(yEncHeader, PATTERN_YENC_HEADER, RegexOptions.Singleline | RegexOptions.Compiled);
                MemoryStream ms = new MemoryStream();
                while(!(line = ReadLine()).StartsWith("=yend")) {
                    YEncDecode(line, ms);
                }
                ms.Position = 0;

                Crc32 crc = new Crc32();
                string expectedHash = line.Substring(line.IndexOf("pcrc32")).Split('=')[1];
                string crcHash = string.Empty;
                crcHash = crc.ComputeHash(ms).Aggregate(crcHash, (a, c) => a += c.ToString("x2"));
                ms.Position = 0;
                ReadLine();

                return new Article {
                    Headers = dict,
                    Body = ms,
                    Filename = m.Groups["name"].Value,
                    Part = int.Parse(m.Groups["part"].Value),
                    ExpectedCrc32 = expectedHash,
                    ActualCrc32 = crcHash
                };
            }

            ArticleNotFound(this, new ArticleNotFoundEventArgs(result, articleId));
            return null;
        }
        /// <summary>
        /// Gets the current UTC date/time on the server.
        /// </summary>
        /// <returns></returns>
        public DateTime Date() {
            var reply = WriteLine("DATE");
            if(reply.IsGood) {
                return DateTime.ParseExact(reply.Message, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            }
            return new DateTime();
        }

        private void CleanUp() {
            if(sr != null && sw != null) {
                sr.Close();
                sr = null;
                sw.Close();
                sw = null;
            }
            Connected = false;
            Authenticated = false;
        }
        private void SetMode(string mode) {
            WriteLine("MODE {0}", mode);
        }
        private string ReadLine() {
            return sr.ReadLine();
        }
        private ServerReply WriteLine(string line, params object[] args) {
            sw.WriteLine(line, args);
            string result = ReadLine();
            var reply = ServerReply.Parse(result);
            return reply;
        }
        private Dictionary<string, string> ReadHeader() {
            var dict = new Dictionary<string, string>();
            string header = null;

            while((header = ReadLine()) != string.Empty && header != ".") {                
                string key = header.Substring(0, header.IndexOf(':'));
                string value = header.Substring(header.IndexOf(':') + 1);
                dict[key] = value.Trim();
            }

            return dict;
        }
        private void YEncDecode(string line, Stream destination) {
            if(line.StartsWith("=ypart")) {
                return;
            }

            byte[] raw = enc.GetBytes(line);
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
        }

        /// <summary>
        /// Gets whether or not the client has been authenticated.
        /// </summary>
        public bool Authenticated { get; private set; }
        /// <summary>
        /// Gets whether or not the client is currently connected to a server.
        /// </summary>
        public bool Connected { get; private set; }
        /// <summary>
        /// Gets the usenet group currently selected.
        /// </summary>
        public string CurrentGroup { get; private set; }
    }
}
