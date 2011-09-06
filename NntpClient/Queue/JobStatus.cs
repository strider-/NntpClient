using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.Queue {
    /// <summary>
    /// Statuses for a Job
    /// </summary>
    public enum JobStatus {
        /// <summary>
        /// Awaiting processing
        /// </summary>
        Queued,
        /// <summary>
        /// Currently being downloaded from usenet
        /// </summary>
        Downloading,
        /// <summary>
        /// Job has been completed, article was downloaded.
        /// </summary>
        Complete,
        /// <summary>
        /// Job failed processing.
        /// </summary>
        Failed
    }
}
