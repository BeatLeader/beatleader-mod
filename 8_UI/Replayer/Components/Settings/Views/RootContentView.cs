using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class RootContentView : ContentView {
        [UIValue("camera-menu-button")] private NavigationButton _cameraMenuButton;
        [UIValue("other-menu-button")] private NavigationButton _otherMenuButton;
        [UIValue("speed-setting")] private SpeedSetting _speedSetting;
        [UIValue("layout-editor-setting")] private LayoutEditorSetting _layoutEditorSetting;

        private OtherContentView _otherContentView;
        private CameraContentView _cameraContentView;

        public void Setup(
            IBeatmapTimeController timeController,
            SongSpeedData speedData,
            ReplayerCameraController cameraController,
            VRControllersAccessor controllersProvider,
            ReplayWatermark watermark,
            LayoutEditor layoutEditor,
            ReplayLaunchData launchData) {
            _speedSetting.Setup(timeController, speedData);
            _layoutEditorSetting.Setup(layoutEditor);
            _cameraContentView.Setup(cameraController, launchData);
            _otherContentView.Setup(controllersProvider, launchData, watermark);
        }

        protected override void OnInstantiate() {
            _speedSetting = Instantiate<SpeedSetting>(transform);
            _layoutEditorSetting = Instantiate<LayoutEditorSetting>(transform);
            _cameraMenuButton = Instantiate<NavigationButton>(transform);
            _otherMenuButton = Instantiate<NavigationButton>(transform);

            _cameraContentView = InstantiateOnSceneRoot<CameraContentView>();
            _otherContentView = InstantiateOnSceneRoot<OtherContentView>();
            _otherContentView.ManualInit(null);
            _cameraContentView.ManualInit(null);

            _cameraMenuButton.Setup(this, _cameraContentView, "Camera");
            _otherMenuButton.Setup(this, _otherContentView, "Other");
        }
    }
}
