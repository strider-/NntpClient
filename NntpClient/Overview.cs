using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient
{
    /// <summary>
    /// Information from the server's overview database for an article.
    /// </summary>
    public class Overview
    {
        private string[] _fields;

        internal Overview(string raw)
        {
            _fields = raw.Split('\t');
        }
        
        /// <summary>
        /// Gets the unique article ID of the article.
        /// </summary>
        public ulong ArticleId { get { return ulong.Parse(_fields[0]); } }
        /// <summary>
        /// Gets the subject of the article
        /// </summary>
        public string Subject { get { return _fields[1]; } }
        /// <summary>
        /// Gets the name of the person who posted the article
        /// </summary>
        public string Author { get { return _fields[2]; } }
        /// <summary>
        /// Gets the date the article was posted (UTC)
        /// </summary>
        public DateTime Date { get { return DateTime.Parse(_fields[3]).ToUniversalTime(); } }
        /// <summary>
        /// Gets the unique message ID of the article
        /// </summary>
        public string MessageId { get { return _fields[4]; } }
        /// <summary>
        /// Gets references to the article
        /// </summary>
        public string References { get { return _fields[5]; } }
        /// <summary>
        /// Gets the size of this article
        /// </summary>
        public uint ByteCount { get { return UInt32.Parse(_fields[6]); } }
        /// <summary>
        /// Gets the number of lines in the body of the article
        /// </summary>
        public uint LineCount { get { return UInt32.Parse(_fields[7]); } }
        /// <summary>
        /// Gets any additional fields included in the overview
        /// </summary>
        public string[] AdditionalFields
        {
            get
            {
                return _fields.Skip(8).ToArray();
            }
        }
    }
}
