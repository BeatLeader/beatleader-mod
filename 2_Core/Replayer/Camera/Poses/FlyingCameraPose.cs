using static BeatLeader.Replayer.InputManager;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using CombinedCameraMovementData = BeatLeader.Models.CombinedCameraMovementData;
using UnityEngine;

namespace BeatLeader.Replayer.Camera
{
    public class FlyingCameraPose : ICameraPoseProvider
    {
        public FlyingCameraPose(Vector2 mouseSensitivity, float flySpeed, bool disableInputOnUnlockedCursor, string name = "FlyingCameraView")
        {
            this.mouseSensitivity = mouseSensitivity;
            this.flySpeed = flySpeed;
            this.disableInputOnUnlockedCursor = disableInputOnUnlockedCursor;
            _name = name;
        }
        public FlyingCameraPose(Vector2 mouseSensitivity, Vector2 lastHeadPos, float flySpeed, bool disableInputOnUnlockedCursor, string name = "FlyingCameraView")
        {
            this.mouseSensitivity = mouseSensitivity;
            _lastHeadPos = lastHeadPos;
            this.flySpeed = flySpeed;
            this.disableInputOnUnlockedCursor = disableInputOnUnlockedCursor;
            _name = name;
        }

        public bool followOrigin = true;
        public bool disableInputOnUnlockedCursor;
        public Vector2 mouseSensitivity;
        public float flySpeed;

        protected Quaternion _lastHeadRot;
        protected Vector3 _lastHeadPos;
        protected Quaternion _lastOriginRot;
        protected Vector3 _lastOriginPos;
        protected bool _returnToTheLastPos;
        protected string _name;

        public InputType AvailableInputs => InputType.FPFC;
        public int Id => 5;
        public bool UpdateEveryFrame => true;
        public string Name => _name;

        public void ProcessPose(ref CombinedCameraMovementData data)
        {
            ref var currentPos = ref data.CameraPose.position;
            ref var currentRot = ref data.CameraPose.rotation;

            bool flag = false;
            if (disableInputOnUnlockedCursor && Cursor.lockState != CursorLockMode.Locked)
            {
                currentPos = _lastHeadPos;
                currentRot = _lastHeadRot;
                flag = true;
            }

            if (!followOrigin)
            {
                currentPos -= data.OriginPose.position - _lastOriginPos;
                currentRot.eulerAngles -= data.OriginPose.rotation.eulerAngles - _lastOriginRot.eulerAngles;
            }

            if (flag) return;

            if (_returnToTheLastPos)
            {
                currentPos = _lastHeadPos;
                currentRot = _lastHeadRot;
                _returnToTheLastPos = false;
            }

            _lastHeadPos = currentPos = GetPosition(currentPos, currentRot);
            _lastHeadRot = currentRot = GetRotation(currentRot);

            _lastOriginPos = data.OriginPose.position;
            _lastOriginRot = data.OriginPose.rotation;
        }
        protected virtual Vector3 GetPosition(Vector3 currentPosition, Quaternion currentRotation)
        {
            Vector3 vector = currentPosition;
            Vector3 a = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                a = currentRotation * Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                a = -(currentRotation * Vector3.forward);
            }
            Vector3 b = Vector3.zero;
            if (Input.GetKey(KeyCode.D))
            {
                b = currentRotation * Vector3.right;
            }
            if (Input.GetKey(KeyCode.A))
            {
                b = -(currentRotation * Vector3.right);
            }
            Vector3 c = Vector3.zero;
            if (Input.GetKey(KeyCode.Space))
            {
                c = currentRotation * Vector3.up;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                c = -(currentRotation * Vector3.up);
            }
            vector += (a + b + c) * flySpeed * Time.deltaTime;
            return vector;
        }
        protected virtual Quaternion GetRotation(Quaternion currentRotation)
        {
            Vector3 rotation = currentRotation.eulerAngles;
            rotation.x += -Input.GetAxis("MouseY") * mouseSensitivity.x;
            rotation.y += Input.GetAxis("MouseX") * mouseSensitivity.y;
            return Quaternion.Euler(rotation);
        }
    }
}
