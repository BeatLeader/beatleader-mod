using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController {
        #region Components

        [UIValue("player-info"), UsedImplicitly]
        private PlayerInfo _playerInfo = ReeUIComponent.Instantiate<PlayerInfo>();

        #endregion
    }
}