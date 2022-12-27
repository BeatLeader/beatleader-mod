using System.Linq;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;
using System.Collections.Generic;
using BeatLeader.Models;
using Vector3 = UnityEngine.Vector3;
using Transform = UnityEngine.Transform;
using BeatLeader.Replayer.Emulation;

namespace BeatLeader.Replayer {
    public class ReplayerCameraController : MonoBehaviour {
        private static readonly IReadOnlyList<ICameraView> FPFCViews = new List<ICameraView>() {
            new StaticCameraView("LeftView", new(-3.70f, 1.70f, 0), new(0, 90, 0)),
            new StaticCameraView("RightView", new(3.70f, 1.70f, 0), new(0, -90, 0)),
            new StaticCameraView("BehindView", new(0f, 1.9f, -2f), Vector3.zero),
            new StaticCameraView("CenterView", new(0f, 1.7f, 0f), Vector3.zero),
            new PlayerViewCameraView(),
            new FlyingCameraView("FreeView", new(0, 1.7f)) {
                mouseSensitivity = new(0.5f, 0.5f),
                flySpeed = 4
            },
        };

        private static readonly IReadOnlyList<ICameraView> VRViews = new List<ICameraView>() {
            new StaticCameraView("LeftView", new(-3.70f, 0, -1.10f), new(0, 60, 0)),
            new StaticCameraView("RightView", new(3.70f, 0, -1.10f), new(0, -60, 0)),
            new StaticCameraView("BehindView", new(0, 0, -2), Vector3.zero),
            new StaticCameraView("CenterView", Vector3.zero, Vector3.zero),
        };

        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly MenuControllersManager _menuControllersManager = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        public IViewableCameraController ViewableCamera => _cameraController;

        [FirstResource]
        private readonly MainSettingsModelSO _mainSettingsModel = null!;

        [FirstResource("VRGameCore", requireActiveInHierarchy: true)]
        private readonly Transform Origin = null!;

        private bool _isInitialized;
        private ViewableCameraController _cameraController = null!;
        private Camera? _camera;

        private Vector3 _handsOriginalPos;
        private Vector3 _handsOriginalRot;

        private void Awake() {
            this.LoadResources();
            _camera = CreateCamera();
            if (_camera == null) {
                Plugin.Log.Error("[Replayer] Failed to initialize Camera!");
                return;
            }
            var cameraTransform = _camera.transform;
            cameraTransform.SetParent(transform, false);
            if (!InputUtils.IsInFPFC) {
                cameraTransform.localPosition = _mainSettingsModel.roomCenter;
                cameraTransform.localEulerAngles = new(0, _mainSettingsModel.roomRotation, 0);
            }
            _cameraController = gameObject.AddComponent<ViewableCameraController>();
            _cameraController.SetCamera(_camera);
            var viewsList = _cameraController.Views;
            foreach (var view in InputUtils.IsInFPFC ? FPFCViews : VRViews) {
                viewsList.Add(view);
            }
            HandlePriorityPlayerChanged(_playersManager.PriorityPlayer!);
            transform.SetParent(Origin, false);
            _camera.enabled = true;
            _isInitialized = true;
        }

        private void Setup() {
            if (InputUtils.IsInFPFC) {
                _camera!.fieldOfView = _launchData.Settings.CameraFOV;
            }
            _cameraController.SetView(_launchData.Settings.ActualCameraView!);

            if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris) {
                _camera!.cullingMask |= 1 << LayerMasks.noteDebrisLayer;
            } else {
                _camera!.cullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
            }
        }

        private void Start() {
            if (!_isInitialized) return;
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            if (!InputUtils.IsInFPFC) {
                var container = _menuControllersManager.HandsContainer;
                _handsOriginalPos = container.localPosition;
                _handsOriginalRot = container.localEulerAngles;
                _cameraController.CameraViewProcessedEvent += HandleViewProcessedEvent;
            }
            Setup();
        }

        private void OnDestroy() {
            _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
        }

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            _cameraController.SetControllers(player.ControllersProvider!);
        }

        private void HandleViewProcessedEvent() {
            var container = _menuControllersManager.HandsContainer;
            container.localPosition = _handsOriginalPos + transform.localPosition;
            container.localEulerAngles = _handsOriginalRot + transform.localEulerAngles;
        }

        private static Camera? CreateCamera() {
            var smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(x => x.transform
                .parent.name == "LocalPlayerGameCore"
                && x.gameObject.activeInHierarchy);

            if (smoothCamera == null) return null;

            var camera = Instantiate(smoothCamera.GetComponent<Camera>(), null, true);
            camera.gameObject.SetActive(false);

            DestroyImmediate(camera.GetComponent<SmoothCameraController>());
            DestroyImmediate(camera.GetComponent<SmoothCamera>());

            camera.nearClipPlane = 0.01f;
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
    }
}
