using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IVirtualPlayer {
        IReplay Replay { get; }
        IReplayScoreEventsProcessor ReplayScoreEventsProcessor { get; }
        IReplayBeatmapEventsProcessor ReplayBeatmapEventsProcessor { get; }
        
        IVRControllersProvider ControllersProvider { get; }
    }
}