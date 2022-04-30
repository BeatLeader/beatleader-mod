using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController {
        #region Components

        [UIValue("status-bar"), UsedImplicitly]
        private StatusBar _statusBar;

        [UIValue("logo"), UsedImplicitly]
        private Logo _logo;

        [UIValue("player-info"), UsedImplicitly]
        private PlayerInfo _playerInfo;

        private void Awake() {
            _statusBar = ReeUIComponentV2.Instantiate<StatusBar>(transform);
            _logo = ReeUIComponentV2.Instantiate<Logo>(transform);
            _playerInfo = ReeUIComponentV2.Instantiate<PlayerInfo>(transform);
        }

        #endregion
    }
}