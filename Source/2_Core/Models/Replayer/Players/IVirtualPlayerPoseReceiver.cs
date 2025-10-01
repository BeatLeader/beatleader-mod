using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    /// <summary>
    /// An abstraction to listen for the virtual player movements.
    /// </summary>
    [PublicAPI]
    public interface IVirtualPlayerPoseReceiver {
        void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose);
    }
}