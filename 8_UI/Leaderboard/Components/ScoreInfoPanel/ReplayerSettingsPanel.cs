using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ReplayerSettingsPanel: ReeUIComponentV2 {
        #region UpdateConfig

        private void UpdateConfig() {
            PluginConfig.ReplayerSettings = new ReplayerSettings {
                showUI = ShowUI,
                showDebris = ShowDebris,
                forceUseReplayerCamera = OverrideCamera,
                loadPlayerEnvironment = UseReplayEnvironment
            };
        }

        #endregion
        
        #region ShowUI

        private bool _showUI = PluginConfig.ReplayerSettings.showUI;

        [UIValue("show-ui"), UsedImplicitly]
        private bool ShowUI {
            get => _showUI;
            set {
                if (_showUI.Equals(value)) return;
                _showUI = value;
                UpdateConfig();
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ShowDebris

        private bool _showDebris = PluginConfig.ReplayerSettings.showDebris;

        [UIValue("show-debris"), UsedImplicitly]
        private bool ShowDebris {
            get => _showDebris;
            set {
                if (_showDebris.Equals(value)) return;
                _showDebris = value;
                UpdateConfig();
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region OverrideCamera

        private bool _overrideCamera = PluginConfig.ReplayerSettings.forceUseReplayerCamera;

        [UIValue("override-camera"), UsedImplicitly]
        private bool OverrideCamera {
            get => _overrideCamera;
            set {
                if (_overrideCamera.Equals(value)) return;
                _overrideCamera = value;
                UpdateConfig();
                NotifyPropertyChanged();
            }
        }

        #endregion
        
        #region UseReplayEnvironment

        private bool _useReplayEnvironment = PluginConfig.ReplayerSettings.loadPlayerEnvironment;

        [UIValue("use-replay-environment"), UsedImplicitly]
        private bool UseReplayEnvironment {
            get => _useReplayEnvironment;
            set {
                if (_useReplayEnvironment.Equals(value)) return;
                _useReplayEnvironment = value;
                UpdateConfig();
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}