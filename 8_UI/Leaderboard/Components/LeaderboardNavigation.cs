using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.LeaderboardNavigation.bsml")]
    internal class LeaderboardNavigation : ReeUiComponent {
        #region Initialize/Dispose

        private LeaderboardEvents _leaderboardEvents;

        public void Initialize(LeaderboardEvents leaderboardEvents) {
            _leaderboardEvents = leaderboardEvents;
            _leaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarted;
            _leaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
        }

        public void Dispose() {
            _leaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarted;
            _leaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
        }

        #endregion

        #region Events

        private void OnScoreRequestStarted() {
            //Disable arrows to prevent multiple requests
        }

        private void OnScoresFetched(Paged<List<Score>> scoresData) {
            //Show arrows if there is more scores
        }

        #endregion

        #region Callbacks

        [UIAction("nav-up-on-click")]
        [UsedImplicitly]
        private void NavUpOnClick() {
            _leaderboardEvents.NotifyUpButtonWasPressed();
        }

        [UIAction("nav-global-on-click")]
        [UsedImplicitly]
        private void NavGlobalOnClick() {
            _leaderboardEvents.NotifyGlobalButtonWasPressed();
        }

        [UIAction("nav-around-on-click")]
        [UsedImplicitly]
        private void NavAroundOnClick() {
            _leaderboardEvents.NotifyAroundButtonWasPressed();
        }

        [UIAction("nav-friends-on-click")]
        [UsedImplicitly]
        private void NavFriendsOnClick() {
            _leaderboardEvents.NotifyFriendsButtonWasPressed();
        }

        [UIAction("nav-country-on-click")]
        [UsedImplicitly]
        private void NavCountryOnClick() {
            _leaderboardEvents.NotifyCountryButtonWasPressed();
        }

        [UIAction("nav-down-on-click")]
        [UsedImplicitly]
        private void NavDownOnClick() {
            _leaderboardEvents.NotifyDownButtonWasPressed();
        }

        #endregion
    }
}