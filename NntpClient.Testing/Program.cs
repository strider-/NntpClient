using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NntpClient.Nzb;
using NntpClient.Queue;
using System.IO;
using System.Net.Sockets;

namespace NntpClient.Testing {
    class Program {
        static void Main(string[] args) {
            var settings = ConfigurationManager.AppSettings;
            string hostname = settings["NntpHost"],
                   user = settings["NntpUser"],
                   pass = settings["NntpPass"];
            int port = int.Parse(settings["NntpPort"]);

            NzbDocument nzb = new NzbDocument(settings["NntpNzb"]);
            DownloadQueue queue = new DownloadQueue(nzb, settings["NntpCachePath"], settings["NntpCompletedPath"]);

            queue.FileCompleted += (s, e) => {
                Console.WriteLine("File Completed ({0})", Path.GetFileName(e.Filename));
            };

            queue.QueueCompleted += (s, e) => {
                Console.WriteLine("Queue Completed");
            };

            using(Client nntp = new Client()) {
                nntp.Connect(hostname, port, true);
                nntp.Authenticate(user, pass);
                nntp.DownloadedChunk += (s, e) => {
                    if(e.Progress == 1f) {
                        Console.WriteLine("{0} {1}/{2}", e.Filename, e.Part, e.Total);
                    }                    
                };

                while(queue.HasJobs) {
                    var item = queue.Pop();
                    var article = nntp.GetArticle(item.ArticleId);

                    if(article != null) {
                        queue.Complete(item, article);
                    } else {
                        queue.Fail(item);
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
