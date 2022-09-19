using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using CombinedCameraMovementData = BeatLeader.Models.CombinedCameraMovementData;
using UnityEngine;
using static BeatLeader.Utils.InputManager;

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
            var camPose = data.cameraPose;
            camPose.position -= offset;

            camPose.position = Vector3.Lerp(camPose.position, data.headPose.position, Time.deltaTime * smoothness);
            camPose.rotation = Quaternion.Lerp(camPose.rotation, data.headPose.rotation, Time.deltaTime * smoothness);

            camPose.position += offset;
            data.cameraPose = camPose;
        }
    }
}
