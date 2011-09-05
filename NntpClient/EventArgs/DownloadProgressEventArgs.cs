using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NntpClient.EventArgs {
    public class DownloadProgressEventArgs : System.EventArgs {
        internal DownloadProgressEventArgs(long downloaded, long size) {
            Downloaded = downloaded;
            Size = size;
        }
        public long Size { get; private set; }
        public long Downloaded { get; private set; }
        public float Progress {
            get {
                if(Size > 0) 
                    return (float)Downloaded / (float)Size;
                return 0;
            }
        }
    }
}
