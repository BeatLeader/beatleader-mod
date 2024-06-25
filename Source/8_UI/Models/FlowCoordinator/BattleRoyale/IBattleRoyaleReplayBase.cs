using BeatLeader.Models;

namespace BeatLeader.UI.Hub {
    public interface IBattleRoyaleReplayBase {
        IReplayHeaderBase ReplayHeader { get; }
        IOptionalReplayData ReplayData { get; }
    }
}