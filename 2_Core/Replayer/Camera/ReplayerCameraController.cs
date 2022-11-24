using System;
using System.Linq;
using System.Collections.Generic;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;
using BeatLeader.Replayer.Emulation;

namespace BeatLeader.Replayer.Camera {
    public class ReplayerCameraController : MonoBehaviour {
        public class InitData {
            public readonly ICameraPoseProvider[] poseProviders;
            public readonly string cameraStartPose;

            public InitData(string cameraStartPose = null) {
                this.cameraStartPose = cameraStartPose;
                poseProviders = new ICameraPoseProvider[0];
            }
            public InitData(string cameraStartPose = null, params ICameraPoseProvider[] poseProviders) {
                this.cameraStartPose = cameraStartPose;
                this.poseProviders = poseProviders;
            }
            public InitData(params ICameraPoseProvider[] poseProviders) {
                this.poseProviders = poseProviders;
            }
        }

        [Inject] protected readonly VRControllersAccessor _vrControllersManager;
        [Inject] protected readonly InitData _data;
        [FirstResource] private readonly MainSettingsModelSO _mainSettingsModel;

        public List<ICameraPoseProvider> PoseProviders { get; protected set; }
        public string CurrentPoseName => _currentPose?.Name ?? string.Empty;
        public UnityEngine.Camera Camera => _camera;
        public ICameraPoseProvider CurrentPose => _currentPose;
        public ValueTuple<Pose, Pose> CameraAndHeadPosesTuple {
            get => (transform.GetLocalPose(), _vrControllersManager.Head.transform.GetLocalPose());
            protected set {
                transform.SetLocalPose(value.Item1);
                _vrControllersManager.Head.transform.SetLocalPose(value.Item2);

                if (InputUtils.IsInFPFC) return;
                _vrControllersManager.HandsContainer.SetLocalPose(value.Item1);
            }
        }
        public int CullingMask {
            get => _camera.cullingMask;
            set => _camera.cullingMask = value;
        }
        public int FieldOfView {
            get => (int)_camera.fieldOfView;
            set {
                if (_fieldOfView == value) return;
                _fieldOfView = value;
                RefreshCamera();
                CameraFOVChangedEvent?.Invoke(value);
            }
        }

        public event Action<ICameraPoseProvider> CameraPoseChangedEvent;
        public event Action<int> CameraFOVChangedEvent;

        protected ICameraPoseProvider _currentPose;
        protected UnityEngine.Camera _camera;

        private int _fieldOfView;
        private bool _wasRequestedLastTime;
        private string _requestedPose;
        private bool _isInitialized;

        private void Awake() {
            if (_data == null || _isInitialized) return;
            this.LoadResources();
            if (!CreateAndAssignCamera()) {
                Plugin.Log.Error("Failed to initialize Replayer Camera!");
                return;
            }
            if (!InputUtils.IsInFPFC)
                ApplyOffsets(_mainSettingsModel.roomCenter, _mainSettingsModel.roomRotation);
            PoseProviders = _data.poseProviders.Where(x => InputUtils.MatchesCurrentInput(x.AvailableInputs)).ToList();
            RequestCameraPose(_data.cameraStartPose);

            transform.SetParent(_vrControllersManager.Origin, false);
            SetEnabled(true);
            _isInitialized = true;
        }
        private void LateUpdate() {
            if (!_isInitialized) return;

            if (_wasRequestedLastTime) {
                SetCameraPose(_requestedPose);
                _wasRequestedLastTime = false;
            }
            if (_currentPose?.UpdateEveryFrame ?? false) {
                CameraAndHeadPosesTuple = ProcessPose(_currentPose);
            }
        }

        public bool RequestCameraPose(string name) {
            if (string.IsNullOrEmpty(name)
                || name == CurrentPoseName) return false;
            _requestedPose = name;
            _wasRequestedLastTime = true;
            return true;
        }
        public bool SetCameraPose(string name) {
            if (string.IsNullOrEmpty(name)
                || name == CurrentPoseName) return false;

            ICameraPoseProvider cameraPose = PoseProviders.FirstOrDefault(x => x.Name == name);
            if (cameraPose == null) return false;

            _currentPose = cameraPose;
            CameraAndHeadPosesTuple = ProcessPose(_currentPose);
            RefreshCamera();
            CameraPoseChangedEvent?.Invoke(cameraPose);
            return true;
        }
        public bool SetCameraPose(ICameraPoseProvider provider) {
            if (PoseProviders.Contains(provider)) return false;
            PoseProviders.Add(provider);
            SetCameraPose(provider.Name);
            return true;
        }
        public void SetEnabled(bool enabled) {
            if (_camera != null) {
                _camera.gameObject.SetActive(enabled);
                _camera.enabled = enabled;
            }
            gameObject.SetActive(enabled);
        }

        protected ValueTuple<Pose, Pose> ProcessPose(ICameraPoseProvider provider) {
            var data = CameraAndHeadPosesTuple;
            _currentPose.ProcessPose(ref data);
            return data;
        }
        protected void RefreshCamera() {
            _camera.stereoTargetEye = InputUtils.IsInFPFC ? StereoTargetEyeMask.None : StereoTargetEyeMask.Both;
            if (InputUtils.IsInFPFC) _camera.fieldOfView = _fieldOfView;
        }
        private bool CreateAndAssignCamera() {
            var smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(x => x.transform.parent.name == "LocalPlayerGameCore");

            if (smoothCamera == null) return false;

            smoothCamera.gameObject.SetActive(false);
            _camera = Instantiate(smoothCamera.GetComponent<UnityEngine.Camera>(), gameObject.transform, true);
            _camera.gameObject.SetActive(false);

            DestroyImmediate(_camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(_camera.GetComponent<SmoothCamera>());

            _camera.nearClipPlane = 0.01f;
            _camera.gameObject.SetActive(true);
            _camera.name = "ReplayerViewCamera";

            return true;
        }
        private void ApplyOffsets(Vector3 pos, float rot) {
            _camera.transform.localPosition = pos;
            _camera.transform.localEulerAngles = new Vector3(0, rot, 0);
        }
    }
}
