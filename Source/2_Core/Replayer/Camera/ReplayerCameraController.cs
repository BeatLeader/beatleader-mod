using System.Linq;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;

namespace BeatLeader.Replayer {
    public class ReplayerCameraController : MonoBehaviour {
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        public IViewableCameraController? ViewableCamera => _cameraController;

        private bool _isInitialized;
        private ViewableCameraController _cameraController = null!;
        private Camera? _camera;

        private void Awake() {
            if (_launchData.Settings.CameraSettings == null) return;
            this.LoadResources();
            _camera = CreateCamera();
            if (_camera == null) {
                Plugin.Log.Error("[Replayer] Failed to initialize Camera!");
                return;
            }
            var cameraTransform = _camera.transform;
            cameraTransform.SetParent(transform, false);
            _cameraController = gameObject.AddComponent<ViewableCameraController>();
            _cameraController.SetCamera(_camera);
            _cameraController.CameraContainer = _extraObjects.ReplayerCore;
            if (_launchData.Settings.CameraSettings.CameraViews is { } views) {
                _cameraController.Views.AddRange(views);
            }
            HandlePriorityPlayerChanged(_playersManager.PriorityPlayer!);
            transform.SetParent(_extraObjects.ReplayerCenterAdjust, false);
            _camera.enabled = true;
            _isInitialized = true;
        }

        private void Start() {
            if (!_isInitialized) return;
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
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
            _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
        }

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            _cameraController.ControllersProvider = player.ControllersProvider!;
        }

        private static Camera? CreateCamera() {
            var smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(x => x.transform
                        .parent.name == "LocalPlayerGameCore"
                    && x.gameObject.activeInHierarchy);

            if (smoothCamera == null) return null;

            var camera = Instantiate(smoothCamera.GetComponent<Camera>(), null, true);
            var cameraGo = camera.gameObject;
            cameraGo.SetActive(false);

            DestroyImmediate(camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(camera.GetComponent<SmoothCamera>());

            camera.nearClipPlane = 0.01f;
            cameraGo.SetActive(true);
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
    }
}