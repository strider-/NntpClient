using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.Decoders {
    internal class DecoderFactory {
        /// <summary>
        /// Determine the binary encoding of a post
        /// </summary>
        /// <param name="connection">Active NNTP connection</param>
        /// <returns></returns>
        public static IBinaryDecoder DetermineDecoder(Connection connection) {
            string header = connection.PeekLine();
            
            if(header.StartsWith("=ybegin")) {
                return new YEncDecoder(connection);
            }

            if(header.StartsWith("begin")) {
                return new Uudecoder(connection);
            }

            return null;
        }
    }
}
