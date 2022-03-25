using System.Collections.Generic;
using BeatLeader.Models;

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
    }
}