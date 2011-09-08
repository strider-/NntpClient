using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NntpClient.Decoders {
    internal interface IBinaryDecoder {
        void Decode(Action<IBinaryDecoder> OnChunkDownloaded);

        MemoryStream Result { get; }
        string Filename { get; }
        string ExpectedCrc32 { get; }
        string ActualCrc32 { get; }
        int BytesRead { get; }
        int ByteOffset { get; }
        int Size { get; }
        int Part { get; }
        int TotalParts { get; }
    }
}
