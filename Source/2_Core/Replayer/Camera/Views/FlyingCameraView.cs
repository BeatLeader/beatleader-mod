using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Replayer {
    public sealed class FlyingCameraView : CameraView {
        public override string Name { get; init; } = "FlyingView";

        public int FlySpeed { get; set; } = 4;
        public SerializableVector2 MouseSensitivity { get; set; } = new Vector2(0.5f, 0.5f);
        
        public SerializableVector3 OriginPosition { get; set; } = new(0, 1.7f, 0);
        public float OriginRotation { get; set; }
        
        public SerializableVector3 ManualPosition { get; set; }
        public float ManualRotation { get; set; }
        
        public bool manualMove;

        private Quaternion _lastRot;
        private Vector3 _lastPos;
        private bool _wasActivatedEarlier;

        public void Reset() {
            if (manualMove) {
                ManualPosition = Vector3.zero;
                ManualRotation = 0;
            } else {
                _lastPos = OriginPosition;
                _lastRot = Quaternion.Euler(0, OriginRotation, 0);
            }
        }

        public override void OnEnable() {
            if (!_wasActivatedEarlier) Reset();
            _wasActivatedEarlier = true;
        }

        public override Pose ProcessPose(Pose headPose) {
            var position = _lastPos;
            var rotation = _lastRot;

            if (!manualMove) {
                if (Cursor.lockState == CursorLockMode.Locked) {
                    position = GetPosition(position, rotation);
                    rotation = GetRotation(rotation);
                }
            } else {
                position = ManualPosition;
                rotation = Quaternion.Euler(0, ManualRotation, 0);
            }

            _lastPos = position;
            _lastRot = rotation;

            return new(position, rotation);
        }

        private Vector3 GetPosition(Vector3 currentPosition, Quaternion currentRotation) {
            var vector = currentPosition;
            var a = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) {
                a = currentRotation * Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S)) {
                a = -(currentRotation * Vector3.forward);
            }
            var b = Vector3.zero;
            if (Input.GetKey(KeyCode.D)) {
                b = currentRotation * Vector3.right;
            }
            if (Input.GetKey(KeyCode.A)) {
                b = -(currentRotation * Vector3.right);
            }
            vector += (a + b) * (FlySpeed * Time.deltaTime);
            return vector;
        }

        private Quaternion GetRotation(Quaternion currentRotation) {
            var rotation = currentRotation.eulerAngles;
            var mouseSensitivity = MouseSensitivity;
            rotation.x += -Input.GetAxis("MouseY") * mouseSensitivity.y;
            rotation.y += Input.GetAxis("MouseX") * mouseSensitivity.x;
            return Quaternion.Euler(rotation);
        }
    }
}