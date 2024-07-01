using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public abstract class CameraView : ICameraView {
        public abstract string Name { get; init; }
        
        public abstract Pose ProcessPose(Pose headPose);

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}