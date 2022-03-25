using System;
using System.Collections.Generic;
using BeatLeader.Models;
using JetBrains.Annotations;

namespace BeatLeader.Manager {
    [UsedImplicitly]
    internal class LeaderboardEvents {
        //-- INPUT ------------------

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

        //-- OUTPUT -----------------

        #region IsLeaderboardVisibleChangedEvent

        // Called when the leaderboard panel visibility changes
        public event Action<bool> IsLeaderboardVisibleChangedEvent;

        public void NotifyIsLeaderboardVisibleChanged(bool value) {
            IsLeaderboardVisibleChangedEvent?.Invoke(value);
        }

        #endregion

        #region UpButtonWasPressed

        public event Action UpButtonWasPressedAction;

        public void NotifyUpButtonWasPressed() {
            UpButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region GlobalButtonWasPressed

        public event Action GlobalButtonWasPressedAction;

        public void NotifyGlobalButtonWasPressed() {
            GlobalButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region AroundButtonWasPressed

        public event Action AroundButtonWasPressedAction;

        public void NotifyAroundButtonWasPressed() {
            AroundButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region FriendsButtonWasPressed

        public event Action FriendsButtonWasPressedAction;

        public void NotifyFriendsButtonWasPressed() {
            FriendsButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region CountryButtonWasPressed

        public event Action CountryButtonWasPressedAction;

        public void NotifyCountryButtonWasPressed() {
            CountryButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region DownButtonWasPressed

        public event Action DownButtonWasPressedAction;

        public void NotifyDownButtonWasPressed() {
            DownButtonWasPressedAction?.Invoke();
        }

        #endregion
    }
}