using BeatLeader.Models;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
namespace BeatLeader.Replayer {
    public class StaticCameraView : ICameraView {
        public StaticCameraView(string name, Vector3 position, Vector3 rotation) {
            Name = name;
            _rotation = Quaternion.Euler(rotation);
            _position = position;
        }

        private Quaternion _rotation;
        private Vector3 _position;

        public bool Update { get; } = false;
        public string Name { get; }

        public void ProcessView(ICameraControllerBase cameraController) {
            var transform = cameraController.CameraContainer;
            transform.localPosition = _position;
            transform.localRotation = _rotation;
        }
    }
}
