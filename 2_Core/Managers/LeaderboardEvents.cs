using System;
using System.Collections.Generic;
using BeatLeader.Models;
using JetBrains.Annotations;

namespace BeatLeader.Manager {
    [UsedImplicitly]
    internal class LeaderboardEvents {
        #region ScoresRequestStarted

        // Called before a score request to server started
        public event Action ScoresRequestStartedEvent;

        public void ScoreRequestStarted() {
            ScoresRequestStartedEvent?.Invoke();
        }

        #endregion

        #region ScoresFetched

        // Called after a score data response is processed
        public event Action<List<Score>> ScoresFetchedEvent;

        public void PublishScores(List<Score> scores) {
            ScoresFetchedEvent?.Invoke(scores);
        }

        #endregion

        #region ProfileRequestStarted

        // Called before a profile request to server started
        public event Action UserProfileStartedEvent;

        public void ProfileRequestStarted() {
            UserProfileStartedEvent?.Invoke();
        }

        #endregion

        #region PublishProfile

        // Called after a profile data response is processed
        public event Action<Profile> UserProfileFetchedEvent;

        public void PublishProfile(Profile profile) {
            UserProfileFetchedEvent?.Invoke(profile);
        }

        #endregion

        #region IsLeaderboardVisibleChangedEvent

        // Called when the leaderboard panel visibility changes
        public event Action<bool> IsLeaderboardVisibleChangedEvent;

        public void NotifyIsLeaderboardVisibleChanged(bool value) {
            IsLeaderboardVisibleChangedEvent?.Invoke(value);
        }

        #endregion
    }
}