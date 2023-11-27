using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IVirtualPlayer {
        IReplay Replay { get; }
        IVRControllersProvider ControllersProvider { get; }
    }
}