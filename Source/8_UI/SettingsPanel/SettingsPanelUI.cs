using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Util;
using JetBrains.Annotations;

namespace BeatLeader {
    internal partial class SettingsPanelUI : NotifiableSingleton<SettingsPanelUI> {
        #region Enabled Toggle

        [UIValue("enabled-value")]
        [UsedImplicitly]
        private bool EnabledValue {
            get => PluginConfig.Enabled;
            set => PluginConfig.Enabled = value;
        }

        #endregion
    }
}