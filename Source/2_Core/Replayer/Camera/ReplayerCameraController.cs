using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;
using BeatLeader.Models;
using IPA.Utilities;
using UnityEngine.SpatialTracking;

namespace BeatLeader.Replayer {
    public class ReplayerCameraController : MonoBehaviour, ICameraController {
        #region Injection

        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;
        [Inject] private readonly DiContainer _diContainer = null!;

        #endregion

        #region Impl

        public IReadOnlyList<ICameraView> Views => _views;
        public ICameraView SelectedView => _cameraView ?? throw new InvalidOperationException();

        public Camera Camera { get; private set; } = null!;

        public event Action<ICameraView>? CameraViewChangedEvent;
        public event Action<int>? CameraFovChangedEvent;

        private IVirtualPlayer? _primaryPlayer;

        public void SetFov(int fov) {
            Camera.fieldOfView = fov;
            _launchData.Settings.CameraSettings!.CameraFOV = fov;
            CameraFovChangedEvent?.Invoke(fov);
        }

        #endregion

        #region Views

        private readonly List<ICameraView> _views = new();
        private ICameraView? _cameraView;

        public void SetView(ICameraView view) {
            _cameraView?.OnDisable();
            _cameraView = view;
            _cameraView.OnEnable();
            _launchData.Settings.CameraSettings!.CameraView = view.Name;
            CameraViewChangedEvent?.Invoke(view);
        }

        private void Update() {
            if (_cameraView == null) return;
            var pose = _primaryPlayer?.MovementProcessor.CurrentMovementFrame.headPose ?? Pose.identity;
            pose = SelectedView.ProcessPose(pose);
            _extraObjects.ReplayerCore.SetLocalPose(pose);
        }

        #endregion

        #region Camera Instantiation

        [FirstResource(ParentName = "LocalPlayerGameCore", RequireActiveInHierarchy = true)]
        private SmoothCamera _smoothCamera = null!;

        private readonly MainCamera _fakeCamera = new();
        private MainCamera _originalCamera = null!;

        private Camera CreateCamera() {
            var camera = Instantiate(_smoothCamera.GetComponent<Camera>(), null, true);
            camera.gameObject.SetActive(false);

            DestroyImmediate(camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(camera.GetComponent<SmoothCamera>());

            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 5000;
            //appending avatar layer
            camera.cullingMask |= 1 << 10;
            camera.gameObject.SetActive(true);
            camera.name = "ReplayerViewCamera";

            if (InputUtils.UsesFPFC) {
                _smoothCamera.gameObject.SetActive(false);
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.fieldOfView = 90;
            } else {
                var trackedPoseDriver = camera.gameObject.AddComponent<TrackedPoseDriver>();
                trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
                trackedPoseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

                camera.stereoTargetEye = StereoTargetEyeMask.Both;
                PatchSmoothCamera();
            }

            return camera;
        }

        private void PatchSmoothCamera() {
            _fakeCamera.SetField("_camera", Camera);
            _fakeCamera.SetField("_transform", _extraObjects.ReplayerCore);

            _originalCamera = _smoothCamera.GetField<MainCamera, SmoothCamera>("_mainCamera");
            _smoothCamera.SetField("_mainCamera", _fakeCamera);
            _smoothCamera.gameObject.SetActive(true);
        }

        private void UnpatchSmoothCamera() {
            if (_originalCamera != null && _smoothCamera != null) {
                _smoothCamera.SetField("_mainCamera", _originalCamera);
            }
        }

        #endregion

        #region Setup

        private void LoadSettings() {
            var cameraSettings = _launchData.Settings.CameraSettings;
            var views = cameraSettings!.CameraViews;
            if (views == null) return;
            foreach (var view in views) {
                _diContainer.Inject(view);
                _views.Add(view);
            }
            if (cameraSettings.CameraView != null) {
                _cameraView = views.FirstOrDefault(x => x.Name == cameraSettings.CameraView);
            }
            if (InputUtils.UsesFPFC) {
                Camera.fieldOfView = _launchData.Settings.CameraSettings!.CameraFOV;
            }
        }

        private void Awake() {
            if (_launchData.Settings.CameraSettings == null) return;
            UnityResourcesHelper.LoadResources(this);

            Camera = CreateCamera();
            Camera.transform.SetParent(transform, false);

            HandlePrimaryPlayerChanged(_playersManager.PrimaryPlayer);
            transform.SetParent(_extraObjects.ReplayerCenterAdjust, false);
            Camera.enabled = true;

            LoadSettings();
            _cameraView ??= _views.FirstOrDefault();
            if (_cameraView != null) SetView(_cameraView);
        }

        private void Start() {
            _playersManager.PrimaryPlayerWasChangedEvent += HandlePrimaryPlayerChanged;

            var playerSettings = _playerDataModel.playerData.playerSpecificSettings;
            var reduceDebris = playerSettings.reduceDebris;
            var mask = Camera.cullingMask;
            mask.SetMaskBit(LayerMasks.noteDebrisLayer, reduceDebris);
            Camera.cullingMask = mask;
        }

        private void OnDestroy() {
            _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChanged;
            if (!InputUtils.UsesFPFC) {
                UnpatchSmoothCamera();
            }
        }

        #endregion

        #region Callbacks

        private void HandlePrimaryPlayerChanged(IVirtualPlayer player) {
            _primaryPlayer = player;
        }

        #endregion
    }
}