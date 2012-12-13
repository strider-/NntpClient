using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using NntpClient.EventArgs;
using NntpClient.Extensions;
using NntpClient.Decoders;

namespace NntpClient {
    /// <summary>
    /// Connects &amp; performs operations on a usenet server.
    /// </summary>
    public class Client : IDisposable {
        Connection conn;
        
        /// <summary>
        /// Fired when an article could not be found on the server
        /// </summary>
        public event EventHandler<ArticleNotFoundEventArgs> ArticleNotFound = delegate { };
        /// <summary>
        /// Fired when a chunk of data has been downloaded from usenet
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> DownloadedChunk = delegate { };

        /// <summary>
        /// Creates a new intance of the client.
        /// </summary>
        public Client() {  }
        /// <summary>
        /// Opens a connection to the given usenet server on the specified port, and whether or not to use ssl.
        /// </summary>
        /// <param name="hostname">Usenet hostname</param>
        /// <param name="port">Port to connect on</param>
        /// <param name="ssl">Uses a secure connection if true</param>
        public void Connect(string hostname, int port, bool ssl) {            
            conn = new Connection(hostname, port, ssl);
            var reply = ServerReply.Parse(conn.ReadLine());
            Connected = reply.IsGood;
        }
        /// <summary>
        /// Closes the connection &amp; returns the statistics of the session.
        /// </summary>
        /// <returns></returns>
        public ConnectionResult Close() {
            if(Connected) {
                string msg = conn.WriteLine("QUIT").Message;
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
            var result = conn.WriteLine("AUTHINFO USER {0}", user);            
            if(result.Code != 381) {
                throw new Exception(result.Message);
            }

            result = conn.WriteLine("AUTHINFO PASS {0}", pass);            
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

            var reply = conn.WriteLine("LIST");

            while((group = conn.ReadLine()) != ".") {
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
            var result = conn.WriteLine("GROUP {0}", groupName);

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
        /// Fetches only the headers of a message with the given ID.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetHeaders(string articleId) {
            var dict = new Dictionary<string, string>();
            var result = conn.WriteLine("HEAD <{0}>", articleId.WithoutBrackets());

            if(result.IsGood) {
                return ReadHeader();
            }
            
            ArticleNotFound(this, new ArticleNotFoundEventArgs(result, articleId));
            return null;
        }

        /// <summary>
        /// Fetches overview database information from a group within a given range of article ids.
        /// A call to SetGroup must be made before calling this method or it will fail.
        /// </summary>
        /// <param name="range">"ArticleId", "ArticleId-" (all articles after this id), or "ArticleId-ArticleId".</param>
        /// <returns></returns>
        public IEnumerable<Overview> GetXOverview(string range)
        {
            var result = conn.WriteLine("XZVER {0}", range);
            if(result.IsGood)
            {
                var decoder = new YEncDecoder(conn);
                decoder.Decode((d) => { });                
                var stream = new DeflateStream(decoder.Result, CompressionMode.Decompress);
                var headers = new StreamReader(stream).ReadToEnd();
                return headers.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(h => new Overview(h));
            }
            return null;
        }
        /// <summary>
        /// Downloads an article from usenet with the given ID. Returns null if the article could not be found.
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public Article GetArticle(string articleId) {
            IBinaryDecoder decoder;
            var result = conn.WriteLine("ARTICLE <{0}>", articleId.WithoutBrackets());

            if(result.IsGood) {
                var dict = ReadHeader();

                decoder = DecoderFactory.DetermineDecoder(conn);
                if(decoder == null) {
                    throw new Exception("Unable to determine binary post encoding.");
                }

                decoder.Decode(
                    d => DownloadedChunk(this, new DownloadProgressEventArgs(d.BytesRead, d.Size, d.Filename, d.Part, d.TotalParts))
                );

                return new Article {
                    Headers = dict,
                    Body = decoder.Result,
                    Filename = decoder.Filename,
                    Part = decoder.Part,
                    TotalParts = decoder.TotalParts,
                    ExpectedCrc32 = decoder.ExpectedCrc32,
                    ActualCrc32 = decoder.ActualCrc32,
                    Start = decoder.ByteOffset
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
            var reply = conn.WriteLine("DATE");
            if(reply.IsGood) {
                return DateTime.ParseExact(reply.Message, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            }
            return new DateTime();
        }
        /// <summary>
        /// Gets whether or not the article is available to download.
        /// </summary>
        /// <param name="articleId">Message-ID of the article</param>
        /// <returns></returns>
        public bool ArticleExists(string articleId) {
            var result = conn.WriteLine("STAT <{0}>", articleId.WithoutBrackets());
            return result.IsGood;
        }

        private void CleanUp() {
            conn.Dispose();
            Connected = false;
            Authenticated = false;
        }
        private void SetMode(string mode) {
            conn.WriteLine("MODE {0}", mode);
        }
        private Dictionary<string, string> ReadHeader() {
            var dict = new Dictionary<string, string>();
            string header = null;

            while((header = conn.ReadLine()) != string.Empty && header != ".") {                
                string key = header.Substring(0, header.IndexOf(':'));
                string value = header.Substring(header.IndexOf(':') + 1);
                dict[key] = value.Trim();
            }

            return dict;
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
