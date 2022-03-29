using System;
using BeatLeader.Models;
using JetBrains.Annotations;

namespace BeatLeader.Manager {
    [UsedImplicitly]
    internal static class LeaderboardEvents {
        //-- INPUT ------------------

        #region ScoresRequestStarted

        // Called before a score request to server started
        public static event Action ScoresRequestStartedEvent;

        public static void ScoreRequestStarted() {
            ScoresRequestStartedEvent?.Invoke();
        }

        #endregion

        #region ScoresFetched

        // Called after a score data response is processed
        public static event Action<Paged<Score>> ScoresFetchedEvent;

        public static void PublishScores(Paged<Score> scores) {
            ScoresFetchedEvent?.Invoke(scores);
        }

        #endregion

        #region ProfileRequestStarted

        // Called before a profile request to server started
        public static event Action UserProfileStartedEvent;

        public static void ProfileRequestStarted() {
            UserProfileStartedEvent?.Invoke();
        }

        #endregion

        #region PublishProfile

        // Called after a profile data response is processed
        public static event Action<Profile> UserProfileFetchedEvent;

        public static void PublishProfile(Profile profile) {
            UserProfileFetchedEvent?.Invoke(profile);
        }

        #endregion

        //-- OUTPUT -----------------

        #region IsLeaderboardVisibleChangedEvent

        // Called when the leaderboard panel visibility changes
        public static event Action<bool> IsLeaderboardVisibleChangedEvent;

        public static void NotifyIsLeaderboardVisibleChanged(bool value) {
            IsLeaderboardVisibleChangedEvent?.Invoke(value);
        }

        #endregion

        #region UpButtonWasPressed

        public static event Action UpButtonWasPressedAction;

        public static void NotifyUpButtonWasPressed() {
            UpButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region GlobalButtonWasPressed

        public static event Action GlobalButtonWasPressedAction;

        public static void NotifyGlobalButtonWasPressed() {
            GlobalButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region AroundButtonWasPressed

        public static event Action AroundButtonWasPressedAction;

        public static void NotifyAroundButtonWasPressed() {
            AroundButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region FriendsButtonWasPressed

        public static event Action FriendsButtonWasPressedAction;

        public static void NotifyFriendsButtonWasPressed() {
            FriendsButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region CountryButtonWasPressed

        public static event Action CountryButtonWasPressedAction;

        public static void NotifyCountryButtonWasPressed() {
            CountryButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region DownButtonWasPressed

        public static event Action DownButtonWasPressedAction;

        public static void NotifyDownButtonWasPressed() {
            DownButtonWasPressedAction?.Invoke();
        }

        #endregion

        public static event Action ProfileRefreshEvent;

        internal static void RequestProfileRefresh() {
            ProfileRefreshEvent?.Invoke();
        }

        public static event Action LeaderboardRefreshEvent;

        internal static void RequestLeaderboardRefresh() {
            LeaderboardRefreshEvent?.Invoke();
        }
    }
}