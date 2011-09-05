using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.EventArgs {
    /// <summary>
    /// Contains event data pertaining to an NNTP reply
    /// </summary>
    public class NntpClientEventArgs : System.EventArgs {
        ServerReply reply;
        internal NntpClientEventArgs(ServerReply reply) {
            this.reply = reply;
        }

        /// <summary>
        /// Gets the reply status code
        /// </summary>
        public int Code { get { return reply.Code; } }
        /// <summary>
        /// Gets the reply message
        /// </summary>
        public string Message { get { return reply.Message; } }
        /// <summary>
        /// Gets whether or not the response represents success
        /// </summary>
        public bool IsGood { get { return reply.IsGood; } }
    }
}
