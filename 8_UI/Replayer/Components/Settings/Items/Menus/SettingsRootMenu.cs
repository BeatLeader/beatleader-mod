using BeatLeader.Replayer;
using BeatLeader.Replayer.Movement;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.SettingsRootMenu.bsml")]
    internal class SettingsRootMenu : MenuWithContainer
    {
        [Inject] private readonly ReplayerCameraController _cameraController;

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
            _cameraMenuButton = CreateButtonForMenu(this, InstantiateInContainer<CameraMenu>(Container), 
                Cam2Util.Detected ? "Camera <color=\"red\">(Cam2 detected)" : "Camera");

            if (Cam2Util.Detected)
            {
                _cameraMenuButton.Interactable = false;
                //PatchCameras();
                _cameraController.SetEnabled(false);
            }
        }
    }
}
