using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBase {
        IReplay Replay { get; }
        IVirtualPlayerMovementProcessor MovementProcessor { get; }
        IReplayScoreEventsProcessor ReplayScoreEventsProcessor { get; }
        IReplayBeatmapEventsProcessor ReplayBeatmapEventsProcessor { get; }
    }
}