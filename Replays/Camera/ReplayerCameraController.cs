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
    public class ReplayerCameraController : MonoBehaviour
    {
        public class InitData
        {
            public ICameraPoseProvider[] poseProviders;
            public readonly int fieldOfView;

            public InitData(int fieldOfView)
            {
                this.fieldOfView = fieldOfView;
                this.poseProviders = new ICameraPoseProvider[0];
            }
            public InitData(int fieldOfView, params ICameraPoseProvider[] poseProviders)
            {
                this.fieldOfView = fieldOfView;
                this.poseProviders = poseProviders;
            }
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly DiContainer _diContainer;
        [Inject] protected readonly InitData _data;

        protected ICameraPoseProvider[] _poseProviders;
        protected ICameraPoseProvider _currentPose;
        protected Camera _camera;
        protected int _fieldOfView;
        protected bool _initialized;

        protected Pose _pose
        {
            get
            {
                return new Pose(transform.position, transform.rotation);
            }
            set
            {
                transform.position = value.position;
                transform.rotation = value.rotation;
                if (!_inputManager.isInFPFC) SetHandsPose(value);
            }
        }

        public ICameraPoseProvider[] poseProviders => _poseProviders;
        public int fieldOfView => _fieldOfView;

        public void Start()
        {
            SmoothCamera smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .First(x => x.transform.parent.name == "LocalPlayerGameCore");
            smoothCamera.gameObject.SetActive(false);
            _camera = Instantiate(smoothCamera.GetComponent<Camera>());

            _camera.gameObject.SetActive(false);
            _camera.name = "ReplayerViewCamera";
            DestroyImmediate(_camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(_camera.GetComponent<SmoothCamera>());
            _camera.transform.SetParent(gameObject.transform);
            _camera.gameObject.SetActive(true);

            _fieldOfView = _inputManager.isInFPFC ? _data.fieldOfView : _fieldOfView;
            _poseProviders = _data.poseProviders.Where(x => x.availableSystems.Contains(_inputManager.currentInputSystem)).ToArray();
            InjectPoses();

            SetEnabled(true);
            _initialized = true;
        }
        public void LateUpdate()
        {
            if (_currentPose == null) return;
            if (_currentPose.updateEveryFrame)
            {
                _pose = _currentPose.GetPose(_pose);
            }
        }
        public void SetCameraFOV(int FOV)
        {
            _fieldOfView = FOV;
            RefreshCamera();
        }
        public void SetCameraPose(string name)
        {
            if (_camera == null) return;
            ICameraPoseProvider cameraPose = null;
            foreach (var item in _poseProviders)
            {
                if (item.name == name)
                {
                    cameraPose = item;
                    break;
                }
            }
            if (cameraPose == null) return;
            _currentPose = cameraPose;
            _pose = _currentPose.GetPose(_pose);
            RefreshCamera();
        }
        public void SetEnabled(bool enabled)
        {
            if (_camera != null)
            {
                _camera.gameObject.SetActive(enabled);
                _camera.enabled = enabled;
            }
            gameObject.SetActive(enabled);
        }

        protected void RefreshCamera()
        {
            if (!_inputManager.isInFPFC)
                _camera.stereoTargetEye = StereoTargetEyeMask.Both;
            else
            { 
                _camera.stereoTargetEye = StereoTargetEyeMask.None;
                _camera.fieldOfView = _fieldOfView;
            }
            SetEnabled(true);
        }
        protected void SetHandsPose(Pose pose)
        {
            _vrControllersManager.handsContainer.transform.position = pose.position;
            _vrControllersManager.handsContainer.transform.rotation = pose.rotation;
        }
        protected void InjectPoses()
        {
            foreach (var item in poseProviders)
            {
                if (item.injectAutomatically)
                    _diContainer.Inject(item);
            }
        }
    }
}
