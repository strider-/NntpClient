using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NntpClient.Nzb {
    /// <summary>
    /// Represents a one-part segment of a file
    /// </summary>
    public class NzbSegment {
        internal NzbSegment(XElement e) {
            Bytes = uint.Parse(e.Attribute("bytes").Value);
            Number = int.Parse(e.Attribute("number").Value);
            ArticleId = e.Value;
        }

        /// <summary>
        /// Gets the size of the article.
        /// </summary>
        public uint Bytes { get; internal set; }
        /// <summary>
        /// Gets the segment number of the article.
        /// </summary>
        public int Number { get; internal set; }
        /// <summary>
        /// Gets the Message-ID of the article.
        /// </summary>
        public string ArticleId { get; internal set; }
    }
}
