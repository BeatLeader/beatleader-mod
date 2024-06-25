using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    internal record BattleRoyaleQueuedReplay(
        IReplayHeaderBase ReplayHeader,
        IOptionalReplayData ReplayData
    ) : IBattleRoyaleQueuedReplay {
        public int ReplayRank { get; set; }
    }
}