using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.Movement;
using UnityEngine.XR;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlayerCameraController : MonoBehaviour
    {
        public class InitData
        {
            public readonly int fieldOfView;
            public readonly int smoothness;
            public readonly bool forceRefresh;

            public InitData(int smoothness, int fieldOfView, bool forceRefresh)
            {
                this.smoothness = smoothness;
                this.fieldOfView = fieldOfView;
                this.forceRefresh = forceRefresh;
            }
        }

        public enum CameraView
        {
            PlayerView,
            LeftView,
            RightView,
            BehindView,
            CenterView,
            CustomView
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly BodyManager _bodyManager;
        [Inject] protected readonly MainCamera _mainCamera;
        [Inject] protected readonly InitData _data;

        protected readonly Vector3 _leftViewPos = new Vector3(-3.70f, 2.30f, -1.10f);
        protected readonly Vector3 _rightViewPos = new Vector3(3.70f, 2.30f, -1.10f);
        protected readonly Vector3 _behindViewPos = new Vector3(0f, 1.9f, -2f);
        protected readonly Vector3 _centerViewPos = new Vector3(0f, 1.7f, 0f);
        protected readonly float _leftViewDir = 45;
        protected readonly float _rightViewDir = -45;
        protected readonly float _behindViewDir;
        protected readonly float _centerViewDir;

        protected Camera _camera;

        protected CameraView _cameraView;
        protected float _smoothness;
        protected int _fieldOfView;
        protected bool _forceRefresh;
        protected bool _initialized;
        protected bool _fpfc;

        public CameraView cameraView => _cameraView;
        public Camera camera => _camera;
        public bool overrideCamera => _camera.enabled;
        public int fieldOfView => _fieldOfView;

        public void Start()
        {
            _camera = Resources.FindObjectsOfTypeAll<SmoothCamera>().First(x => x.transform.parent.name == "LocalPlayerGameCore").GetField<Camera, SmoothCamera>("_camera");
            if (_inputManager.currentInputSystem == InputManager.InputSystemType.FPFC)
            {
                _fpfc = true;
                _smoothness = _data.smoothness;
                _fieldOfView = _data.fieldOfView;
                _forceRefresh = _data.forceRefresh;
                RefreshCamera();
                SetCameraView(CameraView.PlayerView);
            }
            else
            {
                SetCameraView(CameraView.CenterView);
            }
            _initialized = true;
        }
        public void LateUpdate()
        {
            if (_forceRefresh)
            {
                if (_cameraView == CameraView.PlayerView && !_camera.enabled)
                    SetCameraView(_cameraView);
                else if (_cameraView != CameraView.PlayerView)
                    SetCameraView(_cameraView);
            }
            if (_cameraView == CameraView.PlayerView && _fpfc && _bodyManager.initialized)
            {
                Vector3 position = Vector3.Lerp(_camera.transform.position, _bodyManager.head.transform.position, Time.deltaTime * _smoothness);
                Quaternion rotation = Quaternion.Slerp(_camera.transform.rotation, _bodyManager.head.transform.rotation, Time.deltaTime * _smoothness);
                _camera.transform.SetPositionAndRotation(position, rotation);
            }
        }
        public void SetCameraFOV(int FOV)
        {
            if (_initialized)
            {
                _fieldOfView = FOV;
                RefreshCamera();
            }
        }
        public void SetCameraView(CameraView view)
        {
            if (_camera != null)
            {
                switch (_cameraView)
                {
                    case CameraView.PlayerView:
                        if (!_fpfc) return;
                        SetCameraView(Vector3.zero, 0);
                        break;
                    case CameraView.CenterView:
                        SetCameraView(_centerViewPos, _centerViewDir);
                        break;
                    case CameraView.LeftView:
                        SetCameraView(_leftViewPos, _leftViewDir);
                        break;
                    case CameraView.RightView:
                        SetCameraView(_rightViewPos, _rightViewDir);
                        break;
                    case CameraView.BehindView:
                        SetCameraView(_behindViewPos, _behindViewDir);
                        break;
                }
                _cameraView = view;
            }
        }
        public void SetCameraView(Vector3 pos, float dir)
        {
            if (_camera != null)
            {
                _cameraView = CameraView.CustomView;
                _camera.transform.localPosition = pos;
                _camera.transform.localEulerAngles = new Vector3(0, dir, 0);
                RefreshCamera();
            }
        }
        public void SetEnabled(bool enabled)
        {
            _camera?.gameObject.SetActive(enabled);
            gameObject.SetActive(enabled);
        }
        protected void RefreshCamera()
        {
            if (_fpfc && _camera != null)
            {
                _camera.fieldOfView = _fieldOfView;
                _camera.enabled = true;
            }
        }
    }
}
