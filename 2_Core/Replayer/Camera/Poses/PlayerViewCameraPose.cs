using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using UnityEngine;
using static BeatLeader.Utils.InputUtils;
using System;

namespace BeatLeader.Replayer.Camera
{
    internal class PlayerViewCameraPose : ICameraPoseProvider
    {
        public PlayerViewCameraPose(float smoothness, string name = "PlayerView")
        {
            this.smoothness = smoothness;
            _name = name;
        }

        public InputType AvailableInputs => InputType.FPFC;
        public int Id => 4;
        public bool UpdateEveryFrame => true;
        public string Name => _name;

        public Vector3 positionOffset;
        public Quaternion rotationOffset = Quaternion.identity;
        public float smoothness;

        private string _name;

        public void ProcessPose(ref ValueTuple<Pose, Pose> data)
        {
            ref var camPose = ref data.Item1;

            camPose.position -= positionOffset;
            camPose.rotation *= Quaternion.Inverse(rotationOffset);

            camPose.position = Vector3.Lerp(camPose.position, data.Item2.position, Time.deltaTime * smoothness);
            camPose.rotation = Quaternion.Lerp(camPose.rotation, data.Item2.rotation, Time.deltaTime * smoothness);

            camPose.position += positionOffset;
            camPose.rotation *= rotationOffset;
        }
    }
}
