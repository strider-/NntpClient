using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NntpClient.Nzb;

namespace NntpClient.Queue {
    public class DownloadQueue {
        object padLock = new object();
        Queue<QueueItem> queue;

        public DownloadQueue(NzbDocument nzb) {
            queue = new Queue<QueueItem>();

            nzb.Files.SelectMany(
                f => f.Segments,
                (f, s) => new QueueItem {
                    FileID = f.GetHashCode(),
                    Number = s.Number,
                    Total = f.Segments.Count(),
                    ArticleId = s.ArticleId
                }
            ).ToList().ForEach(q => queue.Enqueue(q));
        }

        public QueueItem Pop() {
            lock(padLock) {
                if(queue.Count > 0) {
                    return queue.Dequeue();
                }
                return null;
            }
        }
    }
}
