using System;

namespace BeatLeader.Models {
    [Flags]
    public enum ScoreRowCellType {
        Rank,
        Country,
        Avatar,
        Username,
        Modifiers,
        Accuracy,
        PerformancePoints,
        Score,
        Mistakes
    }
}