using System;
using UnityEngine;
using static BeatLeader.Utils.InputManager;

namespace BeatLeader.Models
{
    public interface ICameraPoseProvider
    {
        public InputType AvailableInputs { get; }
        public bool UpdateEveryFrame { get; }
        public int Id { get; }
        public string Name { get; }

        public void ProcessPose(ref ValueTuple<Pose, Pose> data);
    }
}
