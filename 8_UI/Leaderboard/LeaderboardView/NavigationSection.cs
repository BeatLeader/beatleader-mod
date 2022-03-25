using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader {
    internal partial class LeaderboardView {
        #region Init/Dispose

        private void InitializeNavigationSection() {
            _leaderboardEvents.ScoresRequestStartedEvent += NavigationOnScoreRequestStarted;
            _leaderboardEvents.ScoresFetchedEvent += NavigationOnScoresFetched;
        }

        private void DisposeNavigationSection() {
            _leaderboardEvents.ScoresRequestStartedEvent -= NavigationOnScoreRequestStarted;
            _leaderboardEvents.ScoresFetchedEvent -= NavigationOnScoresFetched;
        }

        #endregion

        #region Events

        private void NavigationOnScoreRequestStarted() {
            //Disable arrows to prevent multiple requests
        }

        private void NavigationOnScoresFetched(List<Score> scores) {
            //Show arrows if there is more scores
        }

        #endregion

        #region Buttons

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