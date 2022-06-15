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
            public readonly ICameraPoseProvider[] poseProviders;
            public readonly string cameraStartPose;
            public readonly int fieldOfView;

            public InitData(int fieldOfView, string cameraStartPose)
            {
                this.fieldOfView = fieldOfView;
                this.cameraStartPose = cameraStartPose;
                poseProviders = new ICameraPoseProvider[0];
            }
            public InitData(int fieldOfView, string cameraStartPose, params ICameraPoseProvider[] poseProviders)
            {
                this.fieldOfView = fieldOfView;
                this.cameraStartPose = cameraStartPose;
                this.poseProviders = poseProviders;
            }
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly DiContainer _diContainer;
        [Inject] protected readonly InitData _data;

        protected List<ICameraPoseProvider> _poseProviders;
        protected ICameraPoseProvider _currentPose;
        protected string _requestedPose;

        protected Camera _camera;
        protected int _fieldOfView;

        protected bool _wasRequestedLastTime;
        protected bool _initialized;

        public List<ICameraPoseProvider> poseProviders => _poseProviders;
        public int fieldOfView => _fieldOfView;
        public Pose pose
        {
            get
            {
                return new Pose(transform.position, transform.rotation);
            }
            protected set
            {
                transform.position = value.position;
                transform.rotation = value.rotation;
                if (!_inputManager.isInFPFC) SetHandsPose(value);
            }
        }

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
            _poseProviders = _data.poseProviders.Where(x => x.availableSystems.Contains(_inputManager.currentInputSystem)).ToList();
            InjectPoses();
            RequestCameraPose(_data.cameraStartPose);

            SetEnabled(true);
            _initialized = true;
        }
        public void LateUpdate()
        {
            if (_wasRequestedLastTime && _initialized)
            {
                SetCameraPose(_requestedPose);
                _wasRequestedLastTime = false;
            }
            if (_currentPose != null && _currentPose.updateEveryFrame)
            {
                pose = _currentPose.GetPose(pose);
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
            pose = _currentPose.GetPose(pose);
            RefreshCamera();
        }
        public void SetCameraPose(ICameraPoseProvider provider)
        {
            if (_camera == null) return;
            ICameraPoseProvider cameraPose = null;
            foreach (var item in _poseProviders)
            {
                if (item == provider)
                {
                    cameraPose = provider;
                    break;
                }
            }

            if (cameraPose == null) _poseProviders.Add(provider);
            _currentPose = provider;
            pose = _currentPose.GetPose(pose);
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
        protected void RequestCameraPose(string name)
        {
            _requestedPose = name;
            _wasRequestedLastTime = true;
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
                if (item.selfInject)
                    _diContainer.Inject(item);
            }
        }
    }
}
