using System;
using System.Linq;
using System.Collections.Generic;
using BeatLeader.Replayer.Movement;
using CameraPoseProvider = BeatLeader.Models.CameraPoseProvider;
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
            public readonly CameraPoseProvider[] poseProviders;
            public readonly string cameraStartPose;

            public InitData(string cameraStartPose = null)
            {
                this.cameraStartPose = cameraStartPose;
                poseProviders = new CameraPoseProvider[0];
            }
            public InitData(string cameraStartPose = null, params CameraPoseProvider[] poseProviders)
            {
                this.cameraStartPose = cameraStartPose;
                this.poseProviders = poseProviders;
            }
            public InitData(params CameraPoseProvider[] poseProviders)
            {
                this.poseProviders = poseProviders;
            }
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly InitData _data;
        [Inject] protected readonly Models.ReplayLaunchData _replayData;

        protected CameraPoseProvider _currentPose;
        protected Camera _camera;
        protected Transform _origin;
        private int _fieldOfView;
        private bool _wasRequestedLastTime;
        private string _requestedPose;

        public event Action<CameraPoseProvider> OnCameraPoseChanged;
        public event Action<int> OnCameraFOVChanged;

        public List<CameraPoseProvider> poseProviders { get; protected set; }
        public CameraPoseProvider CurrentPose => _currentPose;
        public string CurrentPoseName => _currentPose != null ? _currentPose.Name : "NaN";
        public bool IsInitialized { get; private set; }
        public int CullingMask
        {
            get => _camera.cullingMask;
            set => _camera.cullingMask = value;
        }
        public int FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView == value || !_inputManager.IsInFPFC) return;
                _fieldOfView = value;
                RefreshCamera();
                OnCameraFOVChanged?.Invoke(value);
            }
        }
        public CombinedCameraMovementData CombinedMovementData
        {
            get => new CombinedCameraMovementData(transform, _vrControllersManager.Head.transform, _origin);
            protected set
            {
                transform.localPosition = value.cameraPose.position;
                transform.localRotation = value.cameraPose.rotation;

                _vrControllersManager.Head.transform.localPosition = value.headPose.position;
                _vrControllersManager.Head.transform.localRotation = value.headPose.rotation;

                _origin.position = value.originPose.position;
                _origin.rotation = value.originPose.rotation;

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
            _origin = Resources.FindObjectsOfTypeAll<Transform>().First(x => x.gameObject.name == "VRGameCore");
            transform.SetParent(_origin, false);
            _vrControllersManager.HandsContainer.transform.SetParent(_origin, false);

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
            CameraPoseProvider cameraPose = null;
            foreach (var item in poseProviders)
                if (item.Name == name)
                {
                    cameraPose = item;
                    break;
                }
            if (cameraPose == null) return;
            if (_currentPose != null)
                _currentPose.OnPoseUpdateRequested -= RequestUpdate;
            _currentPose = cameraPose;
            _currentPose.OnPoseUpdateRequested += RequestUpdate;
            CombinedMovementData = _currentPose.GetPose(CombinedMovementData);
            RefreshCamera();
            OnCameraPoseChanged?.Invoke(cameraPose);
        }
        public void SetCameraPose(CameraPoseProvider provider)
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
            _camera.fieldOfView = _inputManager.IsInFPFC ? FieldOfView : _camera.fieldOfView;
        }
        protected void RequestCameraPose(string name)
        {
            if (name == string.Empty) return;
            _requestedPose = name;
            _wasRequestedLastTime = true;
        }
        protected void RequestUpdate(CombinedCameraMovementData data)
        {
            CombinedMovementData = data;
        }
        private void SetHandsPose(Pose pose)
        {
            _vrControllersManager.HandsContainer.transform.localPosition = pose.position;
            _vrControllersManager.HandsContainer.transform.localRotation = pose.rotation;
        }
    }
}
