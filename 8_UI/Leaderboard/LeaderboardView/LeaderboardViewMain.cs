using System;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal partial class LeaderboardView : BSMLAutomaticViewController, IInitializable, IDisposable {
        #region Inject

        [Inject] [UsedImplicitly] private LeaderboardEvents _leaderboardEvents;

        #endregion

        #region Init/Dispose

        public void Initialize() {
            InitializeScoresSection();
            InitializeNavigationSection();
        }

        public void Dispose() {
            DisposeScoresSection();
            DisposeNavigationSection();
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