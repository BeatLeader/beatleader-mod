using System;

namespace BeatLeader.Models {
    [Flags]
    internal enum ReplaySaveOption {
        Exit = 0,
        Fail = 1,
        ZeroScore = 2,
        OST = 4
    }
}