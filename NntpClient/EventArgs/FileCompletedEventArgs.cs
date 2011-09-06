using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.EventArgs {
    /// <summary>
    /// Represents a file that has been downloaded from usenet &amp; reassembled.
    /// </summary>
    public class FileCompletedEventArgs : System.EventArgs {
        internal FileCompletedEventArgs(string path, bool broke) {
            Filename = path;
            IsBroken = broke;
        }
        /// <summary>
        /// Gets the location of the completed file.
        /// </summary>
        public string Filename { get; internal set; }
        /// <summary>
        /// Gets whether or not all articles were processed successfully.
        /// </summary>
        public bool IsBroken { get; internal set; }
    }
}
