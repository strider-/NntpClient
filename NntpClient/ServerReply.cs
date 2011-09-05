using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient {
    internal class ServerReply {
        internal ServerReply() { }

        internal static ServerReply Parse(string reply) {
            return new ServerReply {
                Code = int.Parse(reply.Substring(0, 3)),
                Message = reply.Substring(4)
            };
        }

        public int Code { get; private set; }
        public string Message { get; private set; }
        public bool IsGood { get { return Code > 100 && Code < 400; } }
    }
}
