using BeatLeader.Models.AbstractReplay;

namespace BeatLeader.Models {
    public interface IVirtualPlayerMovementProcessor {
        PlayerMovementFrame CurrentMovementFrame { get; }
        
        void AddListener(IVirtualPlayerPoseReceiver poseReceiver);
        void RemoveListener(IVirtualPlayerPoseReceiver poseReceiver);
    }
}