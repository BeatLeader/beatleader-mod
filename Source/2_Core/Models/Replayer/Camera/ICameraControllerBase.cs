namespace BeatLeader.Models {
    public interface ICameraControllerBase {
        IVRControllersProvider ControllersProvider { get; }
        UnityEngine.Transform CameraContainer { get; }
    }
}
