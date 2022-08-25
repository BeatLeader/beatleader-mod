using System.Linq;
using static BeatLeader.Replayer.InputManager;
using BeatLeader.Replayer.Movement;
using CameraPoseProvider = BeatLeader.Models.CameraPoseProvider;
using CombinedCameraMovementData = BeatLeader.Models.CombinedCameraMovementData;
using UnityEngine;

namespace BeatLeader.Replayer.Poses
{
    public class PlayerViewCameraPose : CameraPoseProvider
    {
        public PlayerViewCameraPose(float smoothness, string name = "PlayerView")
        {
            this.smoothness = smoothness;
            _name = name;
        }

        public override InputType AvailableInputs => InputType.FPFC;
        public override int Id => 4;
        public override bool UpdateEveryFrame => true;
        public override string Name => _name;

        public Vector3 offset;
        public float smoothness;

        private string _name;

        public override CombinedCameraMovementData GetPose(CombinedCameraMovementData data)
        {
            var camPose = data.cameraPose;
            camPose.position -= offset;

            camPose.position = Vector3.Lerp(camPose.position, data.headPose.position, Time.deltaTime * smoothness);
            camPose.rotation = Quaternion.Lerp(camPose.rotation, data.headPose.rotation, Time.deltaTime * smoothness);

            camPose.position += offset;
            data.cameraPose = camPose;
            return data;
        }
    }
}
