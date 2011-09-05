using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.Extensions {
    public static class StringExtensions {
        public static int AsInt32(this string str) {
            int val;
            int.TryParse(str, out val);
            return val;
        }
    }
}
