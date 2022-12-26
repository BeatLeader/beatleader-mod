using ICameraView = BeatLeader.Models.ICameraView;
using UnityEngine;

namespace BeatLeader.Replayer {
    public class FlyingCameraView : ICameraView {
        public FlyingCameraView(string name = "FlyingCameraView", Vector2 lastHeadPos = default) {
            Name = name;
            _lastHeadPos = lastHeadPos;
        }

        public Vector2 mouseSensitivity;
        public float flySpeed;

        protected Quaternion _lastHeadRot;
        protected Vector3 _lastHeadPos;
        protected bool _returnToTheLastPos;
        protected bool _resetRequested;

        public bool Update { get; } = true;
        public string Name { get; }

        public void Reset() {
            _resetRequested = true;
        }

        public virtual void ProcessView(Models.ICameraControllerBase cameraController) {
            var transform = cameraController.CameraContainer;
            var position = transform.position;
            var rotatiton = transform.rotation;

            var cursorUnlocked = false;
            if (Cursor.lockState != CursorLockMode.Locked) {
                position = _lastHeadPos;
                rotatiton = _lastHeadRot;
                cursorUnlocked = true;
            }

            if (_resetRequested) {
                position = new Vector3(0, 1.7f, 0);
                rotatiton = Quaternion.identity;
                _resetRequested = false;
            } else if (!cursorUnlocked) {
                position = GetPosition(position, rotatiton);
                rotatiton = GetRotation(rotatiton);
            }

            transform.position = _lastHeadPos = position;
            transform.rotation = _lastHeadRot = rotatiton;
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
