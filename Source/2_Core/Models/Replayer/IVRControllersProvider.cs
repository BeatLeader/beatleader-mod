namespace BeatLeader.Models {
    public interface IVRControllersProvider {
        VRController LeftSaber { get; }
        VRController RightSaber { get; }
        VRController Head { get; }
    }
}