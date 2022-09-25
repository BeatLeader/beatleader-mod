using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components
{
    internal class ReplayerSettingsPanel : ReeUIComponentV2
    {
        #region Components

        [UIValue("hint-field"), UsedImplicitly]
        private HintField _hintField;

        [UIValue("show-ui-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _showUIToggle;

        [UIValue("override-environment-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _overrideEnvironmentToggle;

        [UIValue("save-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _saveToggle;

        private void Awake()
        {
            _hintField = Instantiate<HintField>(transform);
            _showUIToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _overrideEnvironmentToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _saveToggle = Instantiate<ReplayerSettingsToggle>(transform);
        }

        #endregion

        #region OnInitialize

        protected override void OnInitialize()
        {
            _hintField.Setup("<alpha=#66>Player Settings");

            _showUIToggle.Setup(BundleLoader.UIIcon, "Enable UI", _hintField);
            _showUIToggle.Value = PluginConfig.ReplayerSettings.ShowUI;
            _showUIToggle.OnClick += _ => UpdateReplayerSettings();

            _overrideEnvironmentToggle.Setup(BundleLoader.SceneIcon, "Override environment", _hintField);
            _overrideEnvironmentToggle.Value = PluginConfig.ReplayerSettings.LoadPlayerEnvironment;
            _overrideEnvironmentToggle.OnClick += _ => UpdateReplayerSettings();

            _saveToggle.Setup(BundleLoader.SaveIcon, "Save after download", _hintField);
            _saveToggle.Value = PluginConfig.EnableReplayCaching;
            _saveToggle.OnClick += OnSaveToggleValueChanged;
        }

        #endregion

        #region Callbacks

        private static void OnSaveToggleValueChanged(bool value)
        {
            PluginConfig.EnableReplayCaching = value;
        }

        private void UpdateReplayerSettings()
        {
            var settings = PluginConfig.ReplayerSettings;
            settings.ShowUI = _showUIToggle.Value;
            settings.ForceUseReplayerCamera = PluginConfig.ReplayerSettings.ForceUseReplayerCamera;
            settings.LoadPlayerEnvironment = _overrideEnvironmentToggle.Value;

        }

        #endregion
    }
}