using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.EventArgs {
    /// <summary>
    /// Represents the progress of downloading a file segment from usenet
    /// </summary>
    public class DownloadProgressEventArgs : System.EventArgs {
        internal DownloadProgressEventArgs(long downloaded, long size, string name, int part, int total) {
            Downloaded = downloaded;
            Size = size;
            Filename = name;
            Part = part;
            Total = total;
        }
        /// <summary>
        /// Gets the name of the file the current segment belongs to
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// Gets the current part of the file being downloaded
        /// </summary>
        public int Part { get; private set; }
        /// <summary>
        /// Gets the total number of parts for the file
        /// </summary>
        public int Total { get; private set; }
        /// <summary>
        /// Gets the total size of the article to be downloaded
        /// </summary>
        public long Size { get; private set; }
        /// <summary>
        /// Gets the number of bytes downloaded.
        /// </summary>
        public long Downloaded { get; private set; }
        /// <summary>
        /// Gets the percentage of completion.  If the size is unknown, returns 0.
        /// </summary>
        public float Progress {
            get {
                if(Size > 0) 
                    return (float)Downloaded / (float)Size;
                return 0;
            }
        }
    }
}
