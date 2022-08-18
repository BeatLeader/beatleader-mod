using BeatLeader.Replayer.Managers;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.LayoutEditorSetting.bsml")]
    internal class LayoutEditorSetting : Setting
    {
        [InjectOptional] private readonly LayoutEditor _layoutEditor;

        public override int SettingIndex => 2;
        public override Location SettingLocation => Location.Down;
        public override InputManager.InputType AvailableInputs => InputManager.InputType.FPFC;

        [UIAction("button-clicked")]
        private void OpenEditor()
        {
            _layoutEditor?.SetEditModeEnabled(true);
            CloseSettings();
        }
    }
}
