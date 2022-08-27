using static BeatLeader.Replayer.InputManager;
using UnityEngine;
using System;

namespace BeatLeader.Models
{
    public interface ICameraPoseProvider
    {
        public InputType AvailableInputs { get; }
        public bool UpdateEveryFrame { get; }
        public int Id { get; }
        public string Name { get; }

        public CombinedCameraMovementData GetPose(CombinedCameraMovementData data);
    }
}
