namespace BeatLeader.Models {
    public interface IVRControllersProvider : IHandVRControllersProvider {
        VRController Head { get; }
    }
}