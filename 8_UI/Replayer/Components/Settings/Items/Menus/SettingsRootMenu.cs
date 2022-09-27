using BeatLeader.Replayer.Camera;
using BeatLeader.Interop;
using BeatSaberMarkupLanguage.Attributes;
using Zenject;
using BeatLeader.Utils;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.SettingsRootMenu.bsml")]
    internal class SettingsRootMenu : MenuWithContainer
    {
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly Models.ReplayLaunchData _replayData;

        [UIValue("camera-menu-button")] private SubMenuButton _cameraMenuButton;
        [UIValue("body-menu-button")] private SubMenuButton _bodyMenuButton;
        [UIValue("speed-setting")] private SpeedSetting _speedSetting;
        [UIValue("layout-editor-setting")] private LayoutEditorSetting _layoutEditorSetting;

        protected override void OnBeforeParse()
        {
            SetupCameraMenu();
            _bodyMenuButton = CreateButtonForMenu(this, InstantiateInContainer<BodyMenu>(Container), "Body");
            _speedSetting = ReeUIComponentV2WithContainer.InstantiateInContainer<SpeedSetting>(Container, null);
            _layoutEditorSetting = ReeUIComponentV2WithContainer.InstantiateInContainer<LayoutEditorSetting>(Container, null);
            _layoutEditorSetting.EnteredEditModeEvent += () => CloseSettings(false);
        }
        private void SetupCameraMenu()
        {
            var settings = _replayData.actualSettings;
            bool useReplayerCam = settings.ForceUseReplayerCamera;

            bool setupAsCam2 = Cam2Interop.DetectedAndValid && InputManager.IsInFPFC && !useReplayerCam;
            string text = setupAsCam2 ? "Camera <color=\"red\">(Cam2 detected)" : "Camera";
            _cameraMenuButton = CreateButtonForMenu(this, InstantiateInContainer<CameraMenu>(Container), text);

            _cameraMenuButton.Interactable = !setupAsCam2;
            if (setupAsCam2) _cameraController.SetEnabled(false);
        }
    }
}
