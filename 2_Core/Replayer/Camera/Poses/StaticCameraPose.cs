using BeatLeader.Models;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using static BeatLeader.Utils.InputManager;

namespace BeatLeader.Replayer.Camera
{
    public class StaticCameraPose : ICameraPoseProvider
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
        public bool UpdateEveryFrame => false;
        public string Name => _name;

        public void ProcessPose(ref CombinedCameraMovementData data)
        {
            data.cameraPose.position = _position;
            data.cameraPose.rotation = _rotation;
        }
    }
}
