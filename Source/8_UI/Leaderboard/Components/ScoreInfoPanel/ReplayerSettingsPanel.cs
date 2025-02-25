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

        [UIValue("ignore-modifiers-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _ignoreModifiersToggle = null!;
        
        [UIValue("save-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _saveToggle = null!;

        private void Awake() {
            _hintField = Instantiate<HintField>(transform);
            _showUIToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _loadEnvironmentToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _loadJumpDistanceToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _ignoreModifiersToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _saveToggle = Instantiate<ReplayerSettingsToggle>(transform);
        }

        #endregion

        #region SetActive
        
        public void SetActive(bool value) {
            Active = value;
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
        
        #region OnInitialize

        protected override void OnInitialize() {
            _hintField.Setup("<alpha=#66><bll>ls-replayer-settings</bll>");

            _showUIToggle.Setup(BundleLoader.UIIcon, "<bll>ls-replayer-enable-ui</bll>", _hintField);
            _showUIToggle.Value = !PluginConfig.ReplayerSettings.UISettings.ShowUIOnPause;
            _showUIToggle.OnClick += _ => UpdateReplayerSettings();

            _loadEnvironmentToggle.Setup(BundleLoader.SceneIcon, "<bll>ls-replayer-load-environment</bll>", _hintField);
            _loadEnvironmentToggle.Value = PluginConfig.ReplayerSettings.LoadPlayerEnvironment;
            _loadEnvironmentToggle.OnClick += _ => UpdateReplayerSettings();

            _loadJumpDistanceToggle.Setup(BundleLoader.JumpDistanceIcon, "<bll>ls-replayer-load-jd</bll>", _hintField);
            _loadJumpDistanceToggle.Value = PluginConfig.ReplayerSettings.LoadPlayerJumpDistance;
            _loadJumpDistanceToggle.OnClick += _ => UpdateReplayerSettings();
            
            _ignoreModifiersToggle.Setup(BundleLoader.NoModifiersIcon, "<bll>ls-replayer-ignore-modifiers</bll>", _hintField);
            _ignoreModifiersToggle.Value = PluginConfig.ReplayerSettings.IgnoreModifiers;
            _ignoreModifiersToggle.OnClick += _ => UpdateReplayerSettings();

            _saveToggle.Setup(BundleLoader.SaveIcon, "<bll>ls-replayer-store-locally</bll>", _hintField);
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
            settings.UISettings.ShowUIOnPause = !_showUIToggle.Value;
            settings.LoadPlayerEnvironment = _loadEnvironmentToggle.Value;
            settings.LoadPlayerJumpDistance = _loadJumpDistanceToggle.Value;
            settings.IgnoreModifiers = _ignoreModifiersToggle.Value;
        }

        #endregion
    }
}