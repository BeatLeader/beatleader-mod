using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.LeaderboardNavigation.bsml")]
    internal class LeaderboardNavigation : ReeUIComponent {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
        }

        protected override void OnDispose() {
            LeaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
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

        [UIAction("nav-up-on-click"), UsedImplicitly]
        private void NavUpOnClick() {
            LeaderboardEvents.NotifyUpButtonWasPressed();
        }

        [UIAction("nav-global-on-click"), UsedImplicitly]
        private void NavGlobalOnClick() {
            LeaderboardEvents.NotifyGlobalButtonWasPressed();
        }

        [UIAction("nav-around-on-click"), UsedImplicitly]
        private void NavAroundOnClick() {
            LeaderboardEvents.NotifyAroundButtonWasPressed();
        }

        [UIAction("nav-friends-on-click"), UsedImplicitly]
        private void NavFriendsOnClick() {
            LeaderboardEvents.NotifyFriendsButtonWasPressed();
        }

        [UIAction("nav-country-on-click"), UsedImplicitly]
        private void NavCountryOnClick() {
            LeaderboardEvents.NotifyCountryButtonWasPressed();
        }

        [UIAction("nav-down-on-click"), UsedImplicitly]
        private void NavDownOnClick() {
            LeaderboardEvents.NotifyDownButtonWasPressed();
        }

        #endregion
    }
}