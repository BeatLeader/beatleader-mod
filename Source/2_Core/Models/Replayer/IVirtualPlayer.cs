namespace BeatLeader.Models {
    public interface IVirtualPlayer : IVirtualPlayerBase {
        IVRControllersProvider ControllersProvider { get; }
    }
}