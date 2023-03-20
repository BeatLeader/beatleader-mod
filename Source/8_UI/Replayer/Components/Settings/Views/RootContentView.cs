﻿using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class RootContentView : ContentView {
        #region UI Components

        [UIValue("camera-menu-button")]
        private NavigationButton _cameraMenuButton = null!;

        [UIValue("other-menu-button")]
        private NavigationButton _otherMenuButton = null!;

        [UIValue("separator")]
        private HorizontalSeparator _horizontalSeparator = null!;

        [UIValue("speed-setting")]
        private SpeedSetting _speedSetting = null!;

        [UIValue("layout-editor-setting")]
        private LayoutEditorSetting _layoutEditorSetting = null!;

        private OtherContentView _otherContentView = null!;
        private CameraContentView _cameraContentView = null!;

        #endregion

        private bool _useCam2;

        public void Setup(
            IBeatmapTimeController timeController,
            IReplayPauseController pauseController,
            IVirtualPlayersManager playersManager,
            IViewableCameraController cameraController,
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null,
            IReplayTimeline? timeline = null,
            LayoutEditor? layoutEditor = null) {
            _speedSetting.Setup(timeController);
            _layoutEditorSetting.Setup(layoutEditor, pauseController);
            _cameraContentView.Setup(cameraController, launchData);
            _otherContentView.Setup(playersManager, launchData, watermark, timeline);
            cameraController.SetEnabled(!_useCam2);
        }

        protected override void OnInstantiate() {
            _horizontalSeparator = Instantiate<HorizontalSeparator>(transform);
            _speedSetting = Instantiate<SpeedSetting>(transform);
            _layoutEditorSetting = Instantiate<LayoutEditorSetting>(transform);
            _otherMenuButton = Instantiate<NavigationButton>(transform);
            _horizontalSeparator.SeparatorHeight = InputUtils.IsInFPFC ? 10 : 17;
            _otherContentView = InstantiateOnSceneRoot<OtherContentView>();
            _otherContentView.ManualInit(null!);
            _otherMenuButton.Setup(this, _otherContentView, "Other");
            SetupCameraView();
        }

        private void SetupCameraView() {
            _cameraMenuButton = Instantiate<NavigationButton>(transform);
            _cameraContentView = InstantiateOnSceneRoot<CameraContentView>();
            _cameraContentView.ManualInit(null!);
            _useCam2 = Cam2Interop.IsInitialized && InputUtils.IsInFPFC;
            var text = "Camera" + (_useCam2 ? " <color=\"red\">(Cam2 detected)" : "");
            _cameraMenuButton.Setup(this, _cameraContentView, text);
            _cameraMenuButton.Interactable = !_useCam2;
        }
    }
}
