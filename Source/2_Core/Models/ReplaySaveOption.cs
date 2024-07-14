using System;

namespace BeatLeader.Models {
    [Flags]
    internal enum ReplaySaveOption {
        Exit = 1,
        Fail = 2,
        ZeroScore = 4,
        OST = 8,
        Practice = 16
    }
}