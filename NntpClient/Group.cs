using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient {
    /// <summary>
    /// Represents a usenet group
    /// </summary>
    public class Group {
        internal Group() { }
        /// <summary>
        /// Returns the name of the usenet group.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Name;
        }

        internal static Group Parse(string line) {
            string[] group = line.Split(' ');

            return new Group {
                Name = group[0],
                LastArticle = ulong.Parse(group[1]),
                FirstArticle = ulong.Parse(group[2]),
                IsPostingAllowed = group[3] == "y"
            };
        }

        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets the number of the last available article in the group.
        /// </summary>
        public ulong LastArticle { get; internal set; }
        /// <summary>
        /// Gets the number of the first available article in the group.
        /// </summary>
        public ulong FirstArticle { get; internal set; }
        /// <summary>
        /// Gets whether or not posting is allowed in this group.
        /// </summary>
        public bool IsPostingAllowed { get; internal set; }
    }
}
