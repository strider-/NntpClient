using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NntpClient.Nzb;
using NntpClient.Queue;

namespace NntpClient.Testing {
    class Program {
        static void Main(string[] args) {
            var settings = ConfigurationManager.AppSettings;
            string hostname = settings["NntpHost"],
                   user = settings["NntpUser"],
                   pass = settings["NntpPass"];
            int port = int.Parse(settings["NntpPort"]);

            
            using(Client nntp = new Client()) {
                nntp.Connect(hostname, port, true);
                nntp.Authenticate(user, pass);
                nntp.DownloadedChunk += (s, e) => {
                    Console.CursorLeft = 0;
                    Console.CursorTop = 0;
                    Console.Write("{0:P}", e.Progress);
                };
                var art = nntp.GetArticle("part1of1.py$UcS9lvwtFY2bhXcN9@camelsystem-powerpost.local");
                Console.WriteLine();
                Console.WriteLine("E-CRC: {0}", art.ExpectedCrc32);
                Console.WriteLine("A-CRC: {0}", art.ActualCrc32);
                Console.WriteLine("Size:  {0}", art.Body.Length);
                Console.WriteLine("File:  {0}", art.Filename);
                Console.WriteLine("Sgmnt: {0}/{1}", art.Part, art.TotalParts);
            }
             
            Console.ReadLine();
        }
    }
}
