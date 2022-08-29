using BeatLeader.Replayer;
using BeatLeader.Replayer.Camera;
using BeatLeader.Interop;
using BeatSaberMarkupLanguage.Attributes;
using Zenject;

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
            _layoutEditorSetting.OnEnteredEditMode += () => CloseSettings(false);
        }
        private void SetupCameraMenu()
        {
            bool setupAsCam2 = Cam2Interop.Detected && InputManager.IsInFPFC && 
                (!_replayData.overrideSettings || !_replayData.settings.overrideCam2);
            _cameraMenuButton = CreateButtonForMenu(this, InstantiateInContainer<CameraMenu>(Container),
                 setupAsCam2 ? "Camera <color=\"red\">(Cam2 detected)" : "Camera");

            if (setupAsCam2)
            {
                _cameraMenuButton.Interactable = false;
                _cameraController.SetEnabled(false);
            }
            else if (_replayData.overrideSettings && _replayData.settings.overrideCam2)
            {
                bool enable = _replayData.settings.useReplayerCamera;
                _cameraMenuButton.ButtonGameObject.SetActive(enable);
                _cameraController.SetEnabled(enable);
            }
        }
    }
}
