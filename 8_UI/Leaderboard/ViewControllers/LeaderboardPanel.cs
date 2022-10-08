using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController {
        #region Components

        [UIValue("status-bar"), UsedImplicitly]
        private StatusBar _statusBar = ReeUIComponentV2.InstantiateOnSceneRoot<StatusBar>();

        [UIValue("logo"), UsedImplicitly]
        private Logo _logo = ReeUIComponentV2.InstantiateOnSceneRoot<Logo>();

        [UIValue("player-info"), UsedImplicitly]
        private PlayerInfo _playerInfo = ReeUIComponentV2.InstantiateOnSceneRoot<PlayerInfo>();

        private void Awake() {
            _statusBar.SetParent(transform);
            _logo.SetParent(transform);
            _playerInfo.SetParent(transform);
        }

        #endregion
    }
}