using static BeatLeader.Replayer.InputManager;
using BeatLeader.Models;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Pose = UnityEngine.Pose;
using System;

namespace BeatLeader.Replayer.Poses
{
    public class StaticCameraPose : CameraPoseProvider
    {
        public StaticCameraPose(int id, string name, Vector3 position, Quaternion rotation)
        {
            _id = id;
            _rotation = rotation;
            _position = position;
            _name = name;
            _availableInputs = InputType.FPFC | InputType.VR;
        }
        public StaticCameraPose(int id, string name, Vector3 position, Quaternion rotation, InputType availableInputs)
        {
            _id = id;
            _rotation = rotation;
            _position = position;
            _name = name;
            _availableInputs = availableInputs;
        }

        private InputType _availableInputs;
        private Quaternion _rotation;
        private Vector3 _position;
        private int _id;
        private string _name;

        public override InputType AvailableInputs => _availableInputs;
        public override int Id => _id;
        public override bool UpdateEveryFrame => true;
        public override string Name => _name;

        public override CombinedCameraMovementData GetPose(CombinedCameraMovementData data)
        {
            data.cameraPose.position = _position;
            data.cameraPose.rotation = _rotation;
            return data;
        }
    }
}
