using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BeatLeader.Replays.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class FlyingCameraPose : ICameraPoseProvider
    {
        public FlyingCameraPose(Vector2 mouseSensitivity, float flySpeed, bool disableInputOnUnlockedCursor, string name = "FlyingCameraView")
        {
            _mouseSensitivity = mouseSensitivity;
            _flySpeed = flySpeed;
            _disableInputOnUnlockedCursor = disableInputOnUnlockedCursor;
            _name = name;
        }

        protected bool _disableInputOnUnlockedCursor;
        protected Vector2 _mouseSensitivity;
        protected float _flySpeed;

        protected Quaternion _lastHeadRot;
        protected Vector3 _lastHeadPos;
        protected bool _returnToTheLastPos;
        protected string _name;

        public InputSystemType[] availableSystems => new InputSystemType[] { InputSystemType.FPFC };
        public bool injectAutomatically => false;
        public bool updateEveryFrame => true;
        public string name => _name;

        public Pose GetPose(Pose cameraVector)
        {
            if (_disableInputOnUnlockedCursor && Cursor.lockState != CursorLockMode.Locked) return new Pose(_lastHeadPos, _lastHeadRot);

            Vector3 currentPos = cameraVector.position;
            Quaternion currentRot = cameraVector.rotation;

            if (_returnToTheLastPos)
            {
                currentPos = _lastHeadPos;
                currentRot = _lastHeadRot;
                _returnToTheLastPos = false;
            }

            Vector3 position = GetPosition(currentPos, currentRot);
            Quaternion rotation = GetRotation(currentRot);
            _lastHeadPos = position;
            _lastHeadRot = rotation;

            return new Pose(position, rotation);
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
            vector += (a + b + c) * _flySpeed * Time.deltaTime;
            return vector;
        }
        protected virtual Quaternion GetRotation(Quaternion currentRotation)
        {
            Vector3 rotation = currentRotation.eulerAngles;
            rotation.x += -Input.GetAxis("MouseY") * _mouseSensitivity.x;
            rotation.y += Input.GetAxis("MouseX") * _mouseSensitivity.y;
            return Quaternion.Euler(rotation);
        }
    }
}
