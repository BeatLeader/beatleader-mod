using System;
using System.Linq;
using System.Collections.Generic;
using BeatLeader.Replayer.Movement;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using CombinedCameraMovementData = BeatLeader.Models.CombinedCameraMovementData;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;

namespace BeatLeader.Replayer
{
    public class ReplayerCameraController : MonoBehaviour
    {
        public class InitData
        {
            public readonly ICameraPoseProvider[] poseProviders;
            public readonly string cameraStartPose;

            public InitData(string cameraStartPose = null)
            {
                this.cameraStartPose = cameraStartPose;
                poseProviders = new ICameraPoseProvider[0];
            }
            public InitData(string cameraStartPose = null, params ICameraPoseProvider[] poseProviders)
            {
                this.cameraStartPose = cameraStartPose;
                this.poseProviders = poseProviders;
            }
            public InitData(params ICameraPoseProvider[] poseProviders)
            {
                this.poseProviders = poseProviders;
            }
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly InitData _data;
        [Inject] protected readonly Models.ReplayLaunchData _replayData;

        protected ICameraPoseProvider _currentPose;
        protected Camera _camera;

        private int _fieldOfView;
        private bool _wasRequestedLastTime;
        private string _requestedPose;

        public event Action<ICameraPoseProvider> OnCameraPoseChanged;
        public event Action<int> OnCameraFOVChanged;

        public List<ICameraPoseProvider> poseProviders { get; protected set; }
        public ICameraPoseProvider CurrentPose => _currentPose;
        public string CurrentPoseName => _currentPose != null ? _currentPose.Name : "NaN";
        public bool IsInitialized { get; private set; }
        public int CullingMask
        {
            get => _camera.cullingMask;
            set => _camera.cullingMask = value;
        }
        public int FieldOfView
        {
            get => (int)_camera.fieldOfView;
            set
            {
                if (_fieldOfView == value) return;
                _fieldOfView = value;
                RefreshCamera();
                OnCameraFOVChanged?.Invoke(value);
            }
        }
        public CombinedCameraMovementData CombinedMovementData
        {
            get => new CombinedCameraMovementData(transform, _vrControllersManager.Head.transform, _vrControllersManager.OriginTransform);
            protected set
            {
                transform.localPosition = value.cameraPose.position;
                transform.localRotation = value.cameraPose.rotation;

                _vrControllersManager.Head.transform.localPosition = value.headPose.position;
                _vrControllersManager.Head.transform.localRotation = value.headPose.rotation;

                _vrControllersManager.OriginTransform.position = value.originPose.position;
                _vrControllersManager.OriginTransform.rotation = value.originPose.rotation;

                if (!_inputManager.IsInFPFC) SetHandsPose(value.cameraPose);
            }
        }

        private void Awake()
        {
            if (_data == null || IsInitialized) return;
            SmoothCamera smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .First(x => x.transform.parent.name == "LocalPlayerGameCore");
            smoothCamera.gameObject.SetActive(false);
            _camera = Instantiate(smoothCamera.GetComponent<Camera>(), gameObject.transform, true);

            _camera.gameObject.SetActive(false);
            _camera.name = "ReplayerViewCamera";
            DestroyImmediate(_camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(_camera.GetComponent<SmoothCamera>());
            _camera.gameObject.SetActive(true);
            _camera.nearClipPlane = 0.01f;
            //_diContainer.Bind<Camera>().FromInstance(_camera).WithConcreteId("ReplayerCamera").NonLazy();
            transform.SetParent(_vrControllersManager.OriginTransform, false);

            poseProviders = _data.poseProviders.Where(x => _inputManager.MatchesCurrentInput(x.AvailableInputs)).ToList();
            RequestCameraPose(_data.cameraStartPose);

            SetEnabled(_replayData.overrideSettings ? _replayData.settings.useReplayerCamera : true);
            IsInitialized = true;
        }
        private void LateUpdate()
        {
            if (IsInitialized && _wasRequestedLastTime)
            {
                SetCameraPose(_requestedPose);
                _wasRequestedLastTime = false;
            }
            if (_currentPose != null && _currentPose.UpdateEveryFrame)
            {
                CombinedMovementData = _currentPose.GetPose(CombinedMovementData);
            }
        }
        public void SetCameraPose(string name)
        {
            if (_camera == null) return;
            ICameraPoseProvider cameraPose = null;
            foreach (var item in poseProviders)
                if (item.Name == name)
                {
                    cameraPose = item;
                    break;
                }
            if (cameraPose == null) return;
            _currentPose = cameraPose;
            CombinedMovementData = _currentPose.GetPose(CombinedMovementData);
            RefreshCamera();
            OnCameraPoseChanged?.Invoke(cameraPose);
        }
        public void SetCameraPose(ICameraPoseProvider provider)
        {
            if (!poseProviders.Contains(provider))
                poseProviders.Add(provider);
            SetCameraPose(provider.Name);
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
            _camera.stereoTargetEye = _inputManager.IsInFPFC ? StereoTargetEyeMask.None : StereoTargetEyeMask.Both;
            if (_inputManager.IsInFPFC) _camera.fieldOfView = _fieldOfView;
        }
        protected void RequestCameraPose(string name)
        {
            if (name == string.Empty) return;
            _requestedPose = name;
            _wasRequestedLastTime = true;
        }
        private void SetHandsPose(Pose pose)
        {
            _vrControllersManager.HandsContainerTranform.localPosition = pose.position;
            _vrControllersManager.HandsContainerTranform.localRotation = pose.rotation;
        }
    }
}
