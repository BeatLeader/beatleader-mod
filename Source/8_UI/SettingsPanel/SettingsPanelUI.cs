using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Hub;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Util;
using JetBrains.Annotations;
using Reactive;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BeatLeader {
    internal partial class SettingsPanelUI : NotifiableSingleton<SettingsPanelUI> {
        [UIComponent("container"), UsedImplicitly]
        private Transform _container = null!;

        private BeatLeaderSettingsView _settingsView;

        [UIAction("#post-parse"), UsedImplicitly]
        private protected virtual void PostParse() {
            _settingsView = new BeatLeaderSettingsView();
            _settingsView.Use(_container);

            _settingsView.Setup(ConfigFileData.Instance.HubTheme, null);
        }

        [UIAction("#cancel")]
        protected void HandleCancel() {
            _settingsView.CancelSelection();
        }
    }
}