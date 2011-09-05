using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.Queue {
    /// <summary>
    /// Represents a job (file segment) to be processed
    /// </summary>
    public class Job {
        /// <summary>
        /// Gets a unique ID of the overall file being downloaded
        /// </summary>
        public int FileID { get; internal set; }
        /// <summary>
        /// Gets the segment number this job represents
        /// </summary>
        public int Number { get; internal set; }
        /// <summary>
        /// Gets the total number of segments for the file
        /// </summary>
        public int Total { get; internal set; }
        /// <summary>
        /// Gets the message-id to be downloaded
        /// </summary>
        public string ArticleId { get; internal set; }
        /// <summary>
        /// Gets the status of the job
        /// </summary>
        public JobStatus Status { get; internal set; }
        internal string Filename { get; set; }
        internal string CacheLocation { get; set; }
    }
}
