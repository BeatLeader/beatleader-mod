using System;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController, IInitializable, IDisposable {
        #region Components

        [UIValue("player-info"), UsedImplicitly]
        private PlayerInfo _playerInfo = ReeUiComponent.Instantiate<PlayerInfo>();

        #endregion

        #region Init/Dispose

        [Inject, UsedImplicitly]
        private LeaderboardEvents _leaderboardEvents;

        public void Initialize() {
            _playerInfo.Initialize(_leaderboardEvents);
        }

        public void Dispose() {
            _playerInfo.Dispose();
        }

        #endregion
    }
}