using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ReplayerSettingsPanel : ReeUIComponentV2 {
        #region Components

        [UIComponent("labels"), UsedImplicitly]
        private RectTransform _labelsRoot;

        [UIValue("show-ui-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _showUIToggle;

        [UIValue("show-debris-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _showDebrisToggle;

        [UIValue("override-camera-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _overrideCameraToggle;

        [UIValue("override-environment-toggle"), UsedImplicitly]
        private ReplayerSettingsToggle _overrideEnvironmentToggle;

        private void Awake() {
            _showUIToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _showDebrisToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _overrideCameraToggle = Instantiate<ReplayerSettingsToggle>(transform);
            _overrideEnvironmentToggle = Instantiate<ReplayerSettingsToggle>(transform);
        }

        #endregion

        #region OnInitialize

        protected override void OnInitialize() {
            _showUIToggle.Setup(BundleLoader.ReplayIcon, "Enable UI", _labelsRoot);
            _showUIToggle.Value = PluginConfig.ReplayerSettings.showUI;
            _showUIToggle.OnClick += _ => UpdateConfig();

            _showDebrisToggle.Setup(BundleLoader.ReplayIcon, "Enable debris", _labelsRoot);
            _showDebrisToggle.Value = PluginConfig.ReplayerSettings.showDebris;
            _showDebrisToggle.OnClick += _ => UpdateConfig();

            _overrideCameraToggle.Setup(BundleLoader.ReplayIcon, "Override camera", _labelsRoot);
            _overrideCameraToggle.Value = PluginConfig.ReplayerSettings.forceUseReplayerCamera;
            _overrideCameraToggle.OnClick += _ => UpdateConfig();

            _overrideEnvironmentToggle.Setup(BundleLoader.ReplayIcon, "Override environment", _labelsRoot);
            _overrideEnvironmentToggle.Value = PluginConfig.ReplayerSettings.loadPlayerEnvironment;
            _overrideEnvironmentToggle.OnClick += _ => UpdateConfig();
        }

        #endregion

        #region UpdateConfig

        private void UpdateConfig() {
            PluginConfig.ReplayerSettings = new ReplayerSettings {
                showUI = _showUIToggle.Value,
                showDebris = _showDebrisToggle.Value,
                forceUseReplayerCamera = _overrideCameraToggle.Value,
                loadPlayerEnvironment = _overrideEnvironmentToggle.Value
            };
        }

        #endregion
    }
}