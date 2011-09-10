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
            dynamic settings = new Settings();
            NzbDocument nzb = new NzbDocument(settings.Nzb);
            DownloadQueue queue = new DownloadQueue(nzb, settings.CachePath, settings.CompletedPath);

            queue.FileCompleted += (s, e) => {
                lock(padLock) {
                    Console.SetCursorPosition(0, 21);
                    Console.Write("[{0:MM/dd/yyyy hh:mm}] File Completed {1,-96}", DateTime.Now, Path.GetFileName(e.Filename));
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

            Task[] tasks = new Task[settings.MaxConnections];

            for(int i = 0; i < tasks.Length; i++) {
                tasks[i] = new Task((j) => {
                    while(queue.HasJobs) {
                        var item = queue.Pop();

                        if(item != null) {
                            using(var nntp = new Client()) {
                                nntp.Connect(settings.Host, settings.Port, true);
                                nntp.Authenticate(settings.User, settings.Pass);
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
            }
            
            Parallel.ForEach(tasks, task => task.Start());
            Task.WaitAll(tasks);
            Console.SetCursorPosition(0, 22);
            Console.WriteLine("All segments have been downloaded.");
            Console.ReadLine();
        }
    }
}