using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    internal record BattleRoyaleReplay(
        IReplayHeaderBase ReplayHeader,
        IOptionalReplayData ReplayData
    ) : IBattleRoyaleReplay {
        public int ReplayRank { get; set; }
    }
}