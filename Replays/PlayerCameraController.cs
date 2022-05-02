using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
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

            public InitData(int smoothness, int fieldOfView, bool forceRefresh)
            {
                this.smoothness = smoothness;
                this.fieldOfView = fieldOfView;
                this.forceRefresh = forceRefresh;
            }
        }

        [Inject] protected readonly BodyManager _bodyManager;
        [Inject] protected readonly MainCamera _mainCamera;
        [Inject] protected readonly InitData _data;

        protected Camera _camera;
        protected float _smoothness;
        protected float _fieldOfView;
        protected bool _forceRefresh;
        protected bool _initialized;

        public Camera camera => _camera;

        public void Start()
        {
            _camera = Resources.FindObjectsOfTypeAll<SmoothCamera>().First(x => x.transform.parent.name == "LocalPlayerGameCore").GetField<Camera, SmoothCamera>("_camera");
            _mainCamera.camera.fieldOfView = 130;
            _smoothness = _data.smoothness;
            _fieldOfView = _data.fieldOfView;
            _forceRefresh = _data.forceRefresh;
            RefreshPlayerCamera();
        }
        public void LateUpdate()
        {
            if (_forceRefresh && !_camera.enabled) RefreshPlayerCamera();
            if (!_initialized && _bodyManager.initialized)
            {
                _mainCamera.transform.SetParent(_bodyManager.head.transform, false);
                _initialized = true;
            }
            else if (_bodyManager.initialized)
            {
                Vector3 position = Vector3.Lerp(_camera.transform.position, _mainCamera.transform.position, Time.deltaTime * _smoothness);
                Quaternion rotation = Quaternion.Slerp(_camera.transform.rotation, _mainCamera.transform.rotation, Time.deltaTime * _smoothness);
                _camera.transform.SetPositionAndRotation(position, rotation);
            }
        }
        public void RefreshPlayerCamera()
        {
            _camera.fieldOfView = _fieldOfView;
            _camera.enabled = true;
        }
    }
}
