using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.EventArgs {
    /// <summary>
    /// Contains data pertaining to a missing usenet article
    /// </summary>
    public class ArticleNotFoundEventArgs : NntpClientEventArgs {
        internal ArticleNotFoundEventArgs(ServerReply reply, string messageId, ulong articleId)
            : base(reply) {
                MessageId = messageId;
        }

        /// <summary>
        /// Gets the message-id that was not found, if attempting to grab by message-id
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// Gets the article id that was not found, if attempting to grab by article id
        /// </summary>
        public ulong ArticleId { get; private set; }
    }
}
