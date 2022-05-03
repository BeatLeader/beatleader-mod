using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.Movement;
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
            public readonly bool overrideCamera;

            public InitData(int smoothness, int fieldOfView, bool forceRefresh, bool overrideCamera)
            {
                this.smoothness = smoothness;
                this.fieldOfView = fieldOfView;
                this.forceRefresh = forceRefresh;
                this.overrideCamera = overrideCamera;
            }
        }

        public enum PositionalCameraView
        {
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

        protected readonly Vector3 _leftViewPos;
        protected readonly Vector3 _rightViewPos;
        protected readonly Vector3 _behindViewPos;
        protected readonly Vector3 _centerViewPos;
        protected readonly float _leftViewDir;
        protected readonly float _rightViewDir;
        protected readonly float _behindViewDir;
        protected readonly float _centerViewDir;

        protected Camera _playerViewCamera;
        protected Camera _positionalCamera;

        protected PositionalCameraView _positionalCameraView;
        protected float _smoothness;
        protected int _fieldOfView;
        protected bool _forceRefresh;
        protected bool _initialized;
        protected bool _fpfc;

        public PositionalCameraView positionalCameraView => _positionalCameraView;
        public Camera playerViewCamera => _playerViewCamera;
        public Camera positionalCamera => _positionalCamera;
        public bool overrideCamera => _playerViewCamera == null ? false : true;
        public int fieldOfView => _fieldOfView;

        public void Start()
        {
            if (!_data.overrideCamera) return;
            if (_inputManager.currentInputSystem == InputManager.InputSystemType.FPFC)
            {
                _fpfc = true;
                _playerViewCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>().First(x => x.transform.parent.name == "LocalPlayerGameCore").GetField<Camera, SmoothCamera>("_camera");
                _playerViewCamera.fieldOfView = 130;
                _smoothness = _data.smoothness;
                _fieldOfView = _data.fieldOfView;
                _forceRefresh = _data.forceRefresh;
                RefreshPlayerViewCamera();
            }
            else
            {
                _positionalCamera = _mainCamera.camera;
                SetPositionalCameraView(PositionalCameraView.CenterView);
            }
            _initialized = true;
        }
        public void LateUpdate()
        {
            if (_fpfc)
            {
                if (_forceRefresh && !_playerViewCamera.enabled) RefreshPlayerViewCamera();
                if (_bodyManager.initialized)
                {
                    Vector3 position = Vector3.Lerp(_playerViewCamera.transform.position, _bodyManager.head.transform.position, Time.deltaTime * _smoothness);
                    Quaternion rotation = Quaternion.Slerp(_playerViewCamera.transform.rotation, _bodyManager.head.transform.rotation, Time.deltaTime * _smoothness);
                    _playerViewCamera.transform.SetPositionAndRotation(position, rotation);
                }
            }
        }
        public void SetPositionalCameraView(PositionalCameraView view)
        {
            if (!_fpfc && _positionalCamera != null)
            {
                _positionalCameraView = view;
                switch (_positionalCameraView)
                {
                    case PositionalCameraView.CenterView:
                        SetPositionalCameraView(_centerViewPos, _centerViewDir);
                        break;
                    case PositionalCameraView.LeftView:
                        SetPositionalCameraView(_leftViewPos, _leftViewDir);
                        break;
                    case PositionalCameraView.RightView:
                        SetPositionalCameraView(_rightViewPos, _rightViewDir);
                        break;
                    case PositionalCameraView.BehindView:
                        SetPositionalCameraView(_behindViewPos, _behindViewDir);
                        break;
                }
            }
        }
        public void SetPositionalCameraView(Vector3 pos, float dir)
        {
            if (!_fpfc && _positionalCamera != null)
            {
                _positionalCameraView = PositionalCameraView.CustomView;
                _positionalCamera.transform.position = pos;
                _positionalCamera.transform.rotation = new Quaternion(dir, _positionalCamera.transform.rotation.y,
                    _positionalCamera.transform.rotation.z, _positionalCamera.transform.rotation.w);
            }
        }
        public void RefreshPlayerViewCamera()
        {
            if (_fpfc && _playerViewCamera != null)
            {
                _playerViewCamera.fieldOfView = _fieldOfView;
                _playerViewCamera.enabled = true;
            }
        }
        public void SetPlayerViewFOV(int FOV)
        {
            _fieldOfView = FOV;
            RefreshPlayerViewCamera();
        }
        public void SetEnabled(bool enabled)
        {
            _playerViewCamera?.gameObject.SetActive(enabled);
            _positionalCamera?.gameObject.SetActive(enabled);
            this.gameObject.SetActive(enabled);
        }
    }
}
