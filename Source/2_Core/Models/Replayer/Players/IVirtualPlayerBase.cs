using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBase {
        IReplay Replay { get; }
        IReplayScoreEventsProcessor ReplayScoreEventsProcessor { get; }
        IReplayBeatmapEventsProcessor ReplayBeatmapEventsProcessor { get; }
    }
}