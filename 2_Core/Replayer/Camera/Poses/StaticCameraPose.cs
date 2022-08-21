using static BeatLeader.Replayer.Managers.InputManager;
using BeatLeader.Models;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Pose = UnityEngine.Pose;

namespace BeatLeader.Replayer.Poses
{
    public struct StaticCameraPose : ICameraPoseProvider
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

        public InputType AvailableInputs => _availableInputs;
        public int Id => _id;
        public bool SupportsOffset => false;
        public bool UpdateEveryFrame => false;
        public string Name => _name;

        public Pose GetPose(Pose cameraPose) => new Pose(_position, _rotation);
    }
}
