using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NntpClient.Nzb {
    /// <summary>
    /// Represents a file posted to usenet
    /// </summary>
    public class NzbFile {
        internal NzbFile(XElement e, XNamespace n) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long jt = (long)Math.Round(Convert.ToDouble(e.Attribute("date").Value));

            Poster = e.Attribute("poster").Value;
            Date = epoch.AddSeconds(jt).ToLocalTime();
            Subject = e.Attribute("subject").Value;
            Groups = e.Element(n + "groups").Elements(n + "group").Select(g => g.Value).AsEnumerable();
            Segments = e.Element(n + "segments").Elements(n + "segment").Select(s => new NzbSegment(s)).AsEnumerable();
        }

        public override int GetHashCode() {
            return Poster.GetHashCode() ^
                Date.GetHashCode() ^
                Subject.GetHashCode();
        }

        /// <summary>
        /// Gets the person who posted the article.  Copy of the From field of the article header.
        /// </summary>
        public string Poster { get; internal set; }
        /// <summary>
        /// Gets the date &amp; time the article was posted.  From the NZB spec itself, may be unreliable
        /// </summary>
        public DateTime Date { get; internal set; }
        /// <summary>
        /// Gets the article's subject
        /// </summary>
        public string Subject { get; internal set; }
        /// <summary>
        /// Gets a list of usenet groups the segments can be found on
        /// </summary>
        public IEnumerable<string> Groups { get; internal set; }
        /// <summary>
        /// Gets a list of all the segments for this file
        /// </summary>
        public IEnumerable<NzbSegment> Segments { get; internal set; }
    }
}
