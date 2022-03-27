using System;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, IInitializable, IDisposable {
        #region Components

        [UIValue("scores-table"), UsedImplicitly]
        private ScoresTable _scoresTable = ReeUiComponent.Instantiate<ScoresTable>();

        [UIValue("leaderboard-navigation"), UsedImplicitly]
        private LeaderboardNavigation _navigation = ReeUiComponent.Instantiate<LeaderboardNavigation>();

        #endregion

        #region Init/Dispose

        [Inject, UsedImplicitly]
        private LeaderboardEvents _leaderboardEvents;

        public void Initialize() {
            _scoresTable.Initialize(_leaderboardEvents);
            _navigation.Initialize(_leaderboardEvents);
        }

        public void Dispose() {
            _scoresTable.Dispose();
            _navigation.Dispose();
        }

        #endregion

        #region OnEnable & OnDisable

        protected void OnEnable() {
            _leaderboardEvents.NotifyIsLeaderboardVisibleChanged(true);
        }

        protected void OnDisable() {
            _leaderboardEvents.NotifyIsLeaderboardVisibleChanged(false);
        }

        #endregion
    }
}