using System;

namespace BeatLeader.Models {
    [Flags]
    public enum ScoreRowCellType {
        Rank = 0,
        Country = 1,
        Avatar = 2,
        Username = 4,
        Modifiers = 8,
        Accuracy = 16,
        PerformancePoints = 32,
        Score = 64,
        Mistakes = 128
    }
}