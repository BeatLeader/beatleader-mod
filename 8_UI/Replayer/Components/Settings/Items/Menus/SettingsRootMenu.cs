using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.SettingsRootMenu.bsml")]
    internal class SettingsRootMenu : MenuWithContainer
    {
        [UIValue("camera-menu-button")] private SubMenuButton _cameraMenuButton;
        [UIValue("body-menu-button")] private SubMenuButton _bodyMenuButton;
        [UIValue("speed-setting")] private SpeedSetting _speedSetting;
        [UIValue("layout-editor-setting")] private LayoutEditorSetting _layoutEditorSetting;

        protected override void OnBeforeParse()
        {
            _cameraMenuButton = CreateButtonForMenu(this, InstantiateInContainer<CameraMenu>(Container), "Camera");
            _bodyMenuButton = CreateButtonForMenu(this, InstantiateInContainer<BodyMenu>(Container), "Body");
            _speedSetting = ReeUIComponentV2WithContainer.InstantiateInContainer<SpeedSetting>(Container, null);
            _layoutEditorSetting = ReeUIComponentV2WithContainer.InstantiateInContainer<LayoutEditorSetting>(Container, null);
            _layoutEditorSetting.OnEnteredEditMode += () => CloseSettings(false);
        }
    }
}
