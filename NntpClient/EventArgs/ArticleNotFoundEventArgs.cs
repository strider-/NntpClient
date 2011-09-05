using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.EventArgs {
    /// <summary>
    /// Contains data pertaining to a missing usenet article
    /// </summary>
    public class ArticleNotFoundEventArgs : NntpClientEventArgs {
        internal ArticleNotFoundEventArgs(ServerReply reply, string articleId)
            : base(reply) {
                ArticleId = articleId;
        }

        /// <summary>
        /// Gets the message-id that was not found
        /// </summary>
        public string ArticleId { get; private set; }
    }
}
