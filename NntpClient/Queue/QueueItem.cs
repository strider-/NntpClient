using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.Queue {
    public class QueueItem {
        public int FileID { get; internal set; }
        public int Number { get; internal set; }
        public int Total { get; internal set; }
        public string ArticleId { get; internal set; }
    }
}
