using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using UnityEngine;
using static BeatLeader.Utils.InputManager;
using System;

namespace BeatLeader.Replayer.Camera
{
    public class PlayerViewCameraPose : ICameraPoseProvider
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

        public Vector3 offset;
        public float smoothness;

        private string _name;

        public void ProcessPose(ref ValueTuple<Pose, Pose> data)
        {
            var camPose = data.Item1;
            camPose.position -= offset;

            camPose.position = Vector3.Lerp(camPose.position, data.Item2.position, Time.deltaTime * smoothness);
            camPose.rotation = Quaternion.Lerp(camPose.rotation, data.Item2.rotation, Time.deltaTime * smoothness);

            camPose.position += offset;
            data.Item1 = camPose;
        }
    }
}
