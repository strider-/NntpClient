using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient {
    /// <summary>
    /// Contains statistics of a closed usenet session.
    /// </summary>
    public class ConnectionResult {
        internal ConnectionResult() { }

        internal static ConnectionResult Parse(string result) {
            string[] meh = result.Split(' ');
            return new ConnectionResult { 
                TotalBytes = ulong.Parse(meh[1]),
                Articles = uint.Parse(meh[4]),
                Groups = uint.Parse(meh[6])
            };
        }
        /// <summary>
        /// Gets the total number of bytes downloaded during the life of the connection.
        /// </summary>
        public ulong TotalBytes { get; internal set; }
        /// <summary>
        /// Gets the number of articles that were downloaded.
        /// </summary>
        public uint Articles { get; internal set; }
        /// <summary>
        /// Gets the number of groups that were downloaded from.
        /// </summary>
        public uint Groups { get; internal set; }
    }
}
