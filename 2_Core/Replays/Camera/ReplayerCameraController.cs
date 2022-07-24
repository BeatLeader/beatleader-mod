using System;
using System.Linq;
using System.Collections.Generic;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.Movement;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
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

            public InitData(int fieldOfView, string cameraStartPose = null)
            {
                this.fieldOfView = fieldOfView;
                this.cameraStartPose = cameraStartPose;
                poseProviders = new ICameraPoseProvider[0];
            }
            public InitData(int fieldOfView, string cameraStartPose = null, params ICameraPoseProvider[] poseProviders)
            {
                this.fieldOfView = fieldOfView;
                this.cameraStartPose = cameraStartPose;
                this.poseProviders = poseProviders;
            }
            public InitData(int fieldOfView, params ICameraPoseProvider[] poseProviders)
            {
                this.fieldOfView = fieldOfView;
                this.poseProviders = poseProviders;
            }
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly InitData _data;
        [Inject] private readonly DiContainer _diContainer;

        protected ICameraPoseProvider _currentPose;
        protected Camera _camera;
        private int _fieldOfView;
        private bool _wasRequestedLastTime;
        private string _requestedPose;

        public event Action<string> onCameraPoseChanged;
        public event Action<int> onCameraFOVChanged;

        public List<ICameraPoseProvider> poseProviders { get; protected set; }
        public string currentPose => _currentPose != null ? _currentPose.name : "NaN";
        public bool initialized { get; private set; }
        public int cullingMask
        {
            get => _camera.cullingMask;
            set => _camera.cullingMask = value;
        }
        public int fieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView == value) return;
                _fieldOfView = value;
                RefreshCamera();
                onCameraFOVChanged?.Invoke(value);
            }
        }
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

        public void Awake()
        {
            if (_data == null || initialized) return;
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
            //_diContainer.Bind<Camera>().FromInstance(_camera).WithConcreteId("ReplayerCamera").NonLazy();

            fieldOfView = _inputManager.isInFPFC ? _data.fieldOfView : fieldOfView;
            poseProviders = _data.poseProviders.Where(x => x.availableInputs.Contains(_inputManager.currentInputType)).ToList();
            InjectPoses();
            RequestCameraPose(_data.cameraStartPose);

            SetEnabled(true);
            initialized = true;
        }
        private void LateUpdate()
        {
            if (initialized && _wasRequestedLastTime)
            {
                SetCameraPose(_requestedPose);
                _wasRequestedLastTime = false;
            }
            if (_currentPose != null && _currentPose.updateEveryFrame)
            {
                pose = _currentPose.GetPose(pose);
            }
        }
        public void SetCameraPose(string name)
        {
            if (_camera == null) return;
            ICameraPoseProvider cameraPose = null;
            foreach (var item in poseProviders)
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
            onCameraPoseChanged?.Invoke(cameraPose.name);
        }
        public void SetCameraPose(ICameraPoseProvider provider)
        {
            if (!poseProviders.Contains(provider))
                poseProviders.Add(provider);
            SetCameraPose(provider.name);
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
                _camera.fieldOfView = fieldOfView;
            }
            SetEnabled(true);
        }
        protected void RequestCameraPose(string name)
        {
            if (name == String.Empty) return;
            _requestedPose = name;
            _wasRequestedLastTime = true;
        }
        protected void InjectPoses()
        {
            foreach (var item in poseProviders)
            {
                if (item.selfInject)
                    _diContainer.Inject(item);
            }
        }
        private void SetHandsPose(Pose pose)
        {
            _vrControllersManager.handsContainer.transform.position = pose.position;
            _vrControllersManager.handsContainer.transform.rotation = pose.rotation;
        }
    }
}
