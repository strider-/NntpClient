using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NntpClient {
    /// <summary>
    /// Decoded article downloaded from usenet.
    /// </summary>
    public class Article {       
        internal Article() { }
        /// <summary>
        /// Saves the article body to the specified path, using the filename &amp; binary part number in the final location.
        /// </summary>
        /// <param name="path">Directory to store the article in</param>
        /// <returns></returns>
        public string Store(string path) {
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string location = Path.Combine(path, string.Format("{0}.{1}", Filename, Part));
            
            File.WriteAllBytes(location, Body.ToArray());
            Body.Close();
            Body.Dispose();
            
            return location;
        }
        /// <summary>
        /// Gets the article headers
        /// </summary>
        public Dictionary<string, string> Headers { get; internal set; }
        /// <summary>
        /// Gets the yEnc decoded article body.
        /// </summary>
        public MemoryStream Body { get; internal set; }
        /// <summary>
        /// Gets the filename of the article
        /// </summary>
        public string Filename { get; internal set; }
        /// <summary>
        /// Gets the part number of this file segment
        /// </summary>
        public int Part { get; internal set; }
        /// <summary>
        /// Gets the total number of parts the file was split over
        /// </summary>
        public int TotalParts { get; internal set; }
        /// <summary>
        /// Gets whether or not the hashed CRC32 value matches the expected value.
        /// </summary>
        public bool ValidCrc32 { get { return ActualCrc32 == ExpectedCrc32; } }
        /// <summary>
        /// Gets the expected CRC32 value of the article body
        /// </summary>
        public string ExpectedCrc32 { get; internal set; }
        /// <summary>
        /// Gets the hashed CRC32 value of the article body
        /// </summary>
        public string ActualCrc32 { get; internal set; }
        /// <summary>
        /// Gets the offset where this chunk of data starts in the assembled file
        /// </summary>
        public int Start { get; internal set; }
    }
}
