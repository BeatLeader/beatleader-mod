namespace BeatLeader.Models {
    public interface IHandVRControllersProvider {
        VRController LeftHand { get; }
        VRController RightHand { get; }
    }
}