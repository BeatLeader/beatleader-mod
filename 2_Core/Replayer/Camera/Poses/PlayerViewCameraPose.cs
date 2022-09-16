using System.Linq;
using static BeatLeader.Replayer.InputManager;
using BeatLeader.Replayer.Movement;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using CombinedCameraMovementData = BeatLeader.Models.CombinedCameraMovementData;
using UnityEngine;

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

        public void ProcessPose(ref CombinedCameraMovementData data)
        {
            var camPose = data.CameraPose;
            camPose.position -= offset;

            camPose.position = Vector3.Lerp(camPose.position, data.HeadPose.position, Time.deltaTime * smoothness);
            camPose.rotation = Quaternion.Lerp(camPose.rotation, data.HeadPose.rotation, Time.deltaTime * smoothness);

            camPose.position += offset;
            data.CameraPose = camPose;
        }
    }
}
