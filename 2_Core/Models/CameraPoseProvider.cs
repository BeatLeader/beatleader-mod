using static BeatLeader.Replayer.Managers.InputManager;
using UnityEngine;
using System;

namespace BeatLeader.Models
{
    public abstract class CameraPoseProvider
    {
        public virtual InputType AvailableInputs => InputType.VR | InputType.FPFC;
        public virtual bool UpdateEveryFrame => false;
        public abstract int Id { get; }
        public abstract string Name { get; }

        public event Action<Pose> OnPoseUpdateRequested;

        public abstract CombinedCameraMovementData GetPose(CombinedCameraMovementData data);
        protected void RequestUpdate(Pose pose) => OnPoseUpdateRequested?.Invoke(pose);
    }
}
