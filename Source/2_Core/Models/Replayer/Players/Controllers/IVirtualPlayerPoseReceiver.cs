using UnityEngine;

namespace BeatLeader.Models {
    public interface IVirtualPlayerPoseReceiver {
        void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose);
    }
}