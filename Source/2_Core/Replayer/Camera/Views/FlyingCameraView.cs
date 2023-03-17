using ICameraView = BeatLeader.Models.ICameraView;
using UnityEngine;
using System;

namespace BeatLeader.Replayer {
    public class FlyingCameraView : ICameraView {
        public FlyingCameraView(string name = "FlyingCameraView", Vector2 lastPos = default) {
            Name = name;
            _lastPos = lastPos;
        }

        public bool Update { get; } = true;
        public string Name { get; }

        public event Action? ResetEvent;

        public Vector2 mouseSensitivity;
        public float flySpeed;

        public Vector3 manualPosition;
        public float manualRotation;
        public bool manualMove;

        protected Quaternion _lastRot;
        protected Vector3 _lastPos;
        protected bool _returnToTheLastPos;
        protected bool _resetRequested;

        public void Reset() {
            _resetRequested = true;
        }

        public virtual void ProcessView(Models.ICameraControllerBase cameraController) {
            var transform = cameraController.CameraContainer;
            var position = transform.localPosition;
            var rotatiton = transform.localRotation;

            if (!manualMove) {
                var cursorUnlocked = false;
                if (Cursor.lockState != CursorLockMode.Locked) {
                    position = _lastPos;
                    rotatiton = _lastRot;
                    cursorUnlocked = true;
                }
                if (_resetRequested) {
                    position = new Vector3(0, 1.7f, 0);
                    rotatiton = Quaternion.identity;
                    _resetRequested = false;
                    ResetEvent?.Invoke();
                } else if (!cursorUnlocked) {
                    position = GetPosition(position, rotatiton);
                    rotatiton = GetRotation(rotatiton);
                }
            } else {
                if (_resetRequested) {
                    manualPosition = Vector3.zero;
                    manualRotation = 0;
                    _resetRequested = false;
                    ResetEvent?.Invoke();
                }
                position = manualPosition;
                rotatiton = Quaternion.Euler(0, manualRotation, 0);
            }

            transform.localPosition = _lastPos = position;
            transform.localRotation = _lastRot = rotatiton;
        }

        protected virtual Vector3 GetPosition(Vector3 currentPosition, Quaternion currentRotation) {
            Vector3 vector = currentPosition;
            Vector3 a = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) {
                a = currentRotation * Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S)) {
                a = -(currentRotation * Vector3.forward);
            }
            Vector3 b = Vector3.zero;
            if (Input.GetKey(KeyCode.D)) {
                b = currentRotation * Vector3.right;
            }
            if (Input.GetKey(KeyCode.A)) {
                b = -(currentRotation * Vector3.right);
            }
            vector += (a + b) * flySpeed * Time.deltaTime;
            return vector;
        }

        protected virtual Quaternion GetRotation(Quaternion currentRotation) {
            Vector3 rotation = currentRotation.eulerAngles;
            rotation.x += -Input.GetAxis("MouseY") * mouseSensitivity.y;
            rotation.y += Input.GetAxis("MouseX") * mouseSensitivity.x;
            return Quaternion.Euler(rotation);
        }
    }
}
