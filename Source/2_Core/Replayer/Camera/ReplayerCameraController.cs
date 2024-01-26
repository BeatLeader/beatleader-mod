using System.Linq;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;
using BeatLeader.Models;

namespace BeatLeader.Replayer {
    public class ReplayerCameraController : MonoBehaviour, IVirtualPlayerPoseReceiver, IVRControllersProvider {
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        public IViewableCameraController ViewableCamera => _cameraController;

        private ViewableCameraController _cameraController = null!;
        private IVirtualPlayer? _previousPrimaryPlayer;
        private Camera? _camera;
        private bool _isInitialized;

        private void Awake() {
            if (_launchData.Settings.CameraSettings == null) return;
            _camera = CreateCamera();
            if (_camera == null) {
                Plugin.Log.Error("[Replayer] Failed to initialize Camera!");
                return;
            }
            CreateDummies();
            var cameraTransform = _camera.transform;
            cameraTransform.SetParent(transform, false);
            _cameraController = gameObject.AddComponent<ViewableCameraController>();
            _cameraController.SetCamera(_camera);
            _cameraController.CameraContainer = _extraObjects.ReplayerCore;
            _cameraController.ControllersProvider = this;
            if (_launchData.Settings.CameraSettings.CameraViews is { } views) {
                _cameraController.Views.AddRange(views);
            }
            HandlePrimaryPlayerChanged(_playersManager.PrimaryPlayer);
            transform.SetParent(_extraObjects.ReplayerCenterAdjust, false);
            _camera.enabled = true;
            _isInitialized = true;
        }

        private void Start() {
            if (!_isInitialized) return;
            _playersManager.PrimaryPlayerWasChangedEvent += HandlePrimaryPlayerChanged;
            if (InputUtils.IsInFPFC) {
                _camera!.fieldOfView = _launchData.Settings.CameraSettings!.CameraFOV;
            }
            _cameraController.SetView(_launchData.Settings.CameraSettings!.CameraView!);

            if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris) {
                _camera!.cullingMask |= 1 << LayerMasks.noteDebrisLayer;
            } else {
                _camera!.cullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
            }
        }

        private void OnDestroy() {
            _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChanged;
        }

        private void HandlePrimaryPlayerChanged(IVirtualPlayer player) {
            _previousPrimaryPlayer?.MovementProcessor.RemoveListener(this);
            _previousPrimaryPlayer = player;
            _previousPrimaryPlayer.MovementProcessor.AddListener(this);
        }

        private static Camera? CreateCamera() {
            var smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(
                    x => x.transform
                            .parent.name == "LocalPlayerGameCore"
                        && x.gameObject.activeInHierarchy
                );

            if (smoothCamera == null) return null;

            var camera = Instantiate(smoothCamera.GetComponent<Camera>(), null, true);
            camera.gameObject.SetActive(false);

            DestroyImmediate(camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(camera.GetComponent<SmoothCamera>());

            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 5000;
            camera.gameObject.SetActive(true);
            camera.name = "ReplayerViewCamera";

            if (InputUtils.IsInFPFC) {
                smoothCamera.gameObject.SetActive(false);
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.fieldOfView = 90;
            } else {
                camera.stereoTargetEye = StereoTargetEyeMask.Both;
            }

            return camera;
        }

        //TODO: rework camera and remove this temporary logic
        #region Dummies

        public VRController LeftHand { get; private set; } = null!;
        public VRController RightHand { get; private set; } = null!;
        public VRController Head { get; private set; } = null!;

        private void CreateDummies() {
            Head = CreateDummy();
            LeftHand = CreateDummy();
            RightHand = CreateDummy();

            static VRController CreateDummy() {
                var go = new GameObject("ControllerDummy");
                var controller = go.AddComponent<VRController>();
                controller.enabled = false;
                return controller;
            }
        }

        public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
            Head.transform.SetLocalPose(headPose);
            LeftHand.transform.SetLocalPose(leftHandPose);
            RightHand.transform.SetLocalPose(rightHandPose);
        }

        #endregion
    }
}