using System;

namespace BeatLeader.Models {
    [Flags]
    internal enum ScoreInfoPanelTab {
        OverviewPage1 = 0,
        OverviewPage2 = 1,
        Accuracy = 2,
        Grid = 4,
        Graph = 8,
        Replay = 16
    }
}