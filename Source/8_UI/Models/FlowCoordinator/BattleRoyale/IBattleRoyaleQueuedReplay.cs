using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    internal interface IBattleRoyaleQueuedReplay {
        IReplayHeaderBase ReplayHeader { get; }
        IOptionalReplayData ReplayData { get; }
        int ReplayRank { get; }
    }
}