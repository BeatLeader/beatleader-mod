using ICameraView = BeatLeader.Models.ICameraView;
using UnityEngine;

namespace BeatLeader.Replayer {
    public class PlayerViewCameraView : ICameraView {
        public PlayerViewCameraView(string name = "PlayerView", float smoothness = 8) {
            Name = name;
            this.smoothness = smoothness;
        }

        public bool Update => true;
        public string Name { get; }

        public Vector3 positionOffset;
        public Quaternion rotationOffset = Quaternion.identity;
        public float smoothness;

        public void ProcessView(Models.ICameraControllerBase cameraController) {
            var transform = cameraController.CameraContainer;
            var position = transform.localPosition;
            var rotation = transform.localRotation;

            position -= positionOffset;
            rotation *= Quaternion.Inverse(rotationOffset);

            var slerp = Time.deltaTime * smoothness;
            var head = cameraController.ControllersProvider.Head.transform;
            position = Vector3.Lerp(position, head.localPosition, slerp);
            rotation = Quaternion.Lerp(rotation, head.localRotation, slerp);

            transform.localPosition = position += positionOffset;
            transform.localRotation = rotation *= rotationOffset;
        }
    }
}
