using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ReplayerSettingsPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("hint-field"), UsedImplicitly]
        private HintField _hintField = null!;

        [UIValue("show-ui-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _showUIToggle = null!;

        [UIValue("load-environment-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _loadEnvironmentToggle = null!;

        [UIValue("load-jump-distance-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _loadJumpDistanceToggle = null!;

        [UIValue("save-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _saveToggle = null!;

        private void Awake() {
            _hintField = Instantiate<HintField>(transform);
            _showUIToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _loadEnvironmentToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _loadJumpDistanceToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _saveToggle = Instantiate<ReplayerSettingsToggle>(transform);
        }

        #endregion

        #region OnInitialize

        protected override void OnInitialize() {
            _hintField.Setup("<alpha=#66>Player Settings");

            _showUIToggle.Setup(BundleLoader.UIIcon, "Enable UI", _hintField);
            _showUIToggle.Value = !PluginConfig.ReplayerSettings.AutoHideUI;
            _showUIToggle.OnClick += _ => UpdateReplayerSettings();

            _loadEnvironmentToggle.Setup(BundleLoader.SceneIcon, "Load player environment", _hintField);
            _loadEnvironmentToggle.Value = PluginConfig.ReplayerSettings.LoadPlayerEnvironment;
            _loadEnvironmentToggle.OnClick += _ => UpdateReplayerSettings();

            _loadJumpDistanceToggle.Setup(BundleLoader.JumpDistanceIcon, "Load player JD", _hintField);
            _loadJumpDistanceToggle.Value = PluginConfig.ReplayerSettings.LoadPlayerJumpDistance;
            _loadJumpDistanceToggle.OnClick += _ => UpdateReplayerSettings();

            _saveToggle.Setup(BundleLoader.SaveIcon, "Save after download", _hintField);
            _saveToggle.Value = PluginConfig.EnableReplayCaching;
            _saveToggle.OnClick += OnSaveToggleValueChanged;
        }

        #endregion

        #region Callbacks

        private static void OnSaveToggleValueChanged(bool value) {
            PluginConfig.EnableReplayCaching = value;
        }

        private void UpdateReplayerSettings() {
            var settings = PluginConfig.ReplayerSettings;
            settings.AutoHideUI = !_showUIToggle.Value;
            settings.LoadPlayerEnvironment = _loadEnvironmentToggle.Value;
            settings.LoadPlayerJumpDistance = _loadJumpDistanceToggle.Value;
        }

        #endregion
    }
}