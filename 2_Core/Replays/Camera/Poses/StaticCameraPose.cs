using static BeatLeader.Replays.Managers.InputManager;
using BeatLeader.Models;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Pose = UnityEngine.Pose;

namespace BeatLeader.Replays.Poses
{
    public struct StaticCameraPose : ICameraPoseProvider
    {
        public StaticCameraPose()
        {
            _rotation = Quaternion.identity;
            _position = Vector3.zero;
            _name = "NaN";
            _availableInputs = new[] { InputType.FPFC, InputType.VR };
        }
        public StaticCameraPose(string name, Vector3 position, Quaternion rotation)
        {
            _rotation = rotation;
            _position = position;
            _name = name;
            _availableInputs = new[] { InputType.FPFC, InputType.VR };
        }
        public StaticCameraPose(string name, Vector3 position, Quaternion rotation, params InputType[] availableInputs)
        {
            _rotation = rotation;
            _position = position;
            _name = name;
            _availableInputs = availableInputs;
        }

        private InputType[] _availableInputs;
        private Quaternion _rotation;
        private Vector3 _position;
        private string _name;

        public InputType[] availableInputs => _availableInputs;
        public bool selfInject => false;
        public bool updateEveryFrame => false;
        public string name => _name;

        public Pose GetPose(Pose cameraPose) => new Pose(_position, _rotation);
    }
}
