using System;
using System.Linq;
using System.Configuration;
using NntpClient.Nzb;
using NntpClient.Queue;
using System.IO;
using System.Threading.Tasks;

namespace NntpClient.Testing {
    class Program {
        static void Main(string[] args) {
            var padLock = new object();
            var settings = ConfigurationManager.AppSettings;
            string hostname = settings["NntpHost"],
                   user = settings["NntpUser"],
                   pass = settings["NntpPass"];
            int port = int.Parse(settings["NntpPort"]);

            NzbDocument nzb = new NzbDocument(settings["NntpNzb"]);
            DownloadQueue queue = new DownloadQueue(nzb, settings["NntpCachePath"], settings["NntpCompletedPath"]);

            queue.FileCompleted += (s, e) => {
                lock(padLock) {
                    Console.SetCursorPosition(0, 21);
                    Console.Write("[{0:MM/dd/yyyy hh:mm}] File Completed ({1,-102})", DateTime.Now, Path.GetFileName(e.Filename));
                }
            };

            queue.QueueCompleted += (s, e) => {
                lock(padLock) {
                    Console.SetCursorPosition(0, 21);
                    Console.WriteLine("[{0:MM/dd/yyyy hh:mm}] {1,-117}", DateTime.Now, "Queue Completed");
                }
            };
                        
            Console.SetWindowSize(130, 25);
            Console.SetBufferSize(130, 25);
            int maxConnections = int.Parse(settings["NntpMaxConnections"]);
            Task[] tasks = new Task[maxConnections];
            for(int i = 0; i < tasks.Length; i++) {
                tasks[i] = new Task((j) => {
                    while(queue.HasJobs) {
                        var item = queue.Pop();

                        if(item != null) {
                            using(var nntp = new Client()) {
                                nntp.Connect(hostname, port, true);
                                nntp.Authenticate(user, pass);
                                nntp.DownloadedChunk += (s, e) => {
                                    lock(padLock) {
                                        Console.SetCursorPosition(0, (int)j);
                                        Console.Write("Thread {0:00} : {1,-8:P} {4,-80} {2}/{3}", j, e.Progress, e.Part, e.Total, e.Filename);
                                    }
                                };
                                var article = nntp.GetArticle(item.ArticleId);

                                if(article != null)
                                    queue.Complete(item, article);
                                else
                                    queue.Fail(item);
                            }
                        }
                    }
                }, i);
                tasks[i].Start();
            }

            Task.WaitAll(tasks);
            Console.SetCursorPosition(0, 22);
            Console.WriteLine("All segments have been downloaded.");
            Console.ReadLine();
        }
    }
}