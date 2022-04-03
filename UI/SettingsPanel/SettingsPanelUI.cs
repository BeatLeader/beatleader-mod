using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;

namespace BeatLeader {
    public class SettingsPanelUI : NotifiableSingleton<SettingsPanelUI> {
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