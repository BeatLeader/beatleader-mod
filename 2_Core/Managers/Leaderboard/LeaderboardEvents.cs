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

        #region ScoresFetchFailed

        // Called after a score data response is processed
        public static event Action ScoresFetchFailedEvent;

        public static void NotifyScoresFetchFailed() {
            ScoresFetchFailedEvent?.Invoke();
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

        #region ProfileRequestFailed

        public static event Action ProfileRequestFailedEvent;

        public static void NotifyProfileRequestFailed() {
            ProfileRequestFailedEvent?.Invoke();
        }

        #endregion

        #region PublishProfile

        // Called after a profile data response is processed
        public static event Action<Player> UserProfileFetchedEvent;

        public static void PublishProfile(Player profile) {
            UserProfileFetchedEvent?.Invoke(profile);
        }

        #endregion

        #region NotifyUploadStarted
        public static event Action UploadStartedAction;

        public static void NotifyUploadStarted() {
            UploadStartedAction?.Invoke();
        }

        #endregion

        #region NotifyUploadFailed

        public static event Action<bool, int> UploadFailedAction;

        public static void NotifyUploadFailed(bool completely, int retry) {
            UploadFailedAction?.Invoke(completely, retry);
        }

        #endregion

        #region NotifyUploadSuccess
        public static event Action UploadSuccessAction;

        public static void NotifyUploadSuccess() {
            UploadSuccessAction?.Invoke();
        }

        # endregion
        
        #region ShowStatusMessage
        public static event Action<string, float> StatusMessageEvent;

        public static void ShowStatusMessage(string message, float duration) {
            StatusMessageEvent?.Invoke(message, duration);
        }

        # endregion

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

        #region AroundButtonWasPressed

        public static event Action AroundButtonWasPressedAction;

        public static void NotifyAroundButtonWasPressed() {
            AroundButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region DownButtonWasPressed

        public static event Action DownButtonWasPressedAction;

        public static void NotifyDownButtonWasPressed() {
            DownButtonWasPressedAction?.Invoke();
        }

        #endregion

        #region ScopeWasSelected

        public static event Action<ScoresScope> ScopeWasSelectedAction;

        public static void NotifyScopeWasSelected(ScoresScope scope) {
            ScopeWasSelectedAction?.Invoke(scope);
        }

        #endregion
        
        //-- INTERNAL -----------------

        #region ScoreInfoButtonWasPressed

        public static event Action<Score> ScoreInfoButtonWasPressed;

        public static void NotifyScoreInfoButtonWasPressed(Score score) {
            ScoreInfoButtonWasPressed?.Invoke(score);
        }

        #endregion
    }
}