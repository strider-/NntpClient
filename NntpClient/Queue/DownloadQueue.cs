using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NntpClient.Nzb;
using System.IO;
using NntpClient.EventArgs;

namespace NntpClient.Queue {
    /// <summary>
    /// Provides a queue for downloading files in an NZB.
    /// </summary>
    public class DownloadQueue {
        object padLock = new object();
        Dictionary<int, bool> assembled;
        List<Job> queue;

        /// <summary>
        /// Fired when a file has been downloaded successfully.
        /// </summary>
        public event EventHandler<FileCompletedEventArgs> FileCompleted = delegate { };
        /// <summary>
        /// Fired when all jobs have been processed.
        /// </summary>
        public event EventHandler QueueCompleted = delegate { };

        /// <summary>
        /// Initializes the queue with an NZB, a directory to store temp files, and a directory to store completed files.
        /// </summary>
        /// <param name="nzb">NzbDocument to process</param>
        /// <param name="cachePath">Location to store file segments until they can be assembled</param>
        /// <param name="completedPath">Location to store completed files.</param>
        public DownloadQueue(NzbDocument nzb, string cachePath, string completedPath) {            
            CacheDirectory = new DirectoryInfo(Path.Combine(cachePath, nzb.Name));
            if(!CacheDirectory.Exists)
                CacheDirectory.Create();

            CompletedDirectory = new DirectoryInfo(Path.Combine(completedPath, nzb.Name));
            if(!CompletedDirectory.Exists)
                CompletedDirectory.Create();

            queue = nzb.Files.SelectMany(
                f => f.Segments,
                (f, s) => new Job {
                    FileID = f.GetHashCode(),
                    Number = s.Number,
                    Total = f.Segments.Count(),
                    ArticleId = s.ArticleId,
                    Status = JobStatus.Queued                   
                }
            ).ToList();

            assembled = queue.GroupBy(q => q.FileID).Select(g => g.Key).ToDictionary(k => k, v => false);
        }

        /// <summary>
        /// Returns the next job to be processed, or null if there are no jobs left.
        /// </summary>
        /// <returns></returns>
        public Job Pop() {
            if(HasJobs) {
                lock(padLock) {
                    if(HasJobs) {
                        var job = queue.First(q => q.Status == JobStatus.Queued);
                        job.Status = JobStatus.Downloading;
                        return job;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Marks a job as complete, requires the completed article.
        /// </summary>
        /// <param name="job">Job to complete</param>
        /// <param name="article">Article of the completed job</param>
        public void Complete(Job job, Article article) {
            job.Status = JobStatus.Complete;
            job.Filename = article.Filename;
            job.CacheLocation = article.Store(CacheDirectory.FullName);
            job.ByteOffset = article.Start;

            if(FileDownloaded(job.FileID) && !assembled[job.FileID]) {
                lock(padLock) {
                    if(FileDownloaded(job.FileID) && !assembled[job.FileID]) {
                        AssembleFile(job.FileID);
                    }
                }
            }
        }
        /// <summary>
        /// Marks a job as failed
        /// </summary>
        /// <param name="job"></param>
        public void Fail(Job job) {
            job.Status = JobStatus.Failed;
        }

        private bool FileDownloaded(int FileId) {
            return queue.Where(q => q.FileID == FileId).All(q => q.Status == JobStatus.Complete || q.Status == JobStatus.Failed);
        }
        private void AssembleFile(int FileId) {
            var segments = queue.Where(q => q.FileID == FileId).OrderBy(q => q.Number);            
            string filename = segments.First().Filename;
            bool broken = segments.Any(s => s.Status == JobStatus.Failed);
            
            if(broken) {
                filename = filename.ToUpper();
            }
            
            string path = Path.Combine(CompletedDirectory.FullName, filename);
            using(FileStream file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                foreach(var segment in segments) {
                    if(segment.Status == JobStatus.Complete) {
                        using(FileStream part = new FileStream(segment.CacheLocation, FileMode.Open, FileAccess.Read, FileShare.None)) {
                            file.Position = segment.ByteOffset;
                            part.CopyTo(file);
                        }
                        File.Delete(segment.CacheLocation);
                    }
                }
            }
            
            assembled[FileId] = true;
            FileCompleted(this, new FileCompletedEventArgs(path, broken));
            if(assembled.All(f => f.Value)) {
                CacheDirectory.Delete(true);
                QueueCompleted(this, System.EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the directory file segments will be placed in
        /// </summary>
        public DirectoryInfo CacheDirectory { get; private set; }
        /// <summary>
        /// Gets the directory completed files will be placed in
        /// </summary>
        public DirectoryInfo CompletedDirectory { get; private set; }
        /// <summary>
        /// Gets whether or not there are more jobs to process
        /// </summary>
        public bool HasJobs { get { return queue.Where(q => q.Status == JobStatus.Queued).Count() > 0; } }
    }
}
