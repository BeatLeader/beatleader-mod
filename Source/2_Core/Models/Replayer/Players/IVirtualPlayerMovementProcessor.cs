namespace BeatLeader.Models {
    public interface IVirtualPlayerMovementProcessor {
        void AddListener(IVirtualPlayerPoseReceiver poseReceiver);
        void RemoveListener(IVirtualPlayerPoseReceiver poseReceiver);
    }
}