using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.Extensions {
    /// <summary>
    /// Provides additional string methods
    /// </summary>
    public static class StringExtensions {
        /// <summary>
        /// Converts a string into an int32 value.
        /// </summary>
        /// <param name="str">string to convert</param>
        /// <returns></returns>
        public static int AsInt32(this string str) {
            int val;
            int.TryParse(str, out val);
            return val;
        }
        /// <summary>
        /// Trims angled brackets from the start and end of a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string WithoutBrackets(this string str) {
            return str.Trim('<', '>');
        }
    }
}
