using System;
using BeatLeader.Models;
using JetBrains.Annotations;

namespace BeatLeader.Manager {
    [UsedImplicitly]
    internal static class LeaderboardEvents {
        //-- INPUT ------------------

        #region ShowStatusMessage

        public static event Action<string, float> StatusMessageEvent;

        public static void ShowStatusMessage(string message, float duration = 1f) {
            StatusMessageEvent?.Invoke(message, duration);
        }

        # endregion

        #region SceneTransitionStarted

        public static event Action SceneTransitionStartedEvent;

        public static void NotifySceneTransitionStarted() {
            SceneTransitionStartedEvent?.Invoke();
        }

        # endregion

        //-- OUTPUT -----------------

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

        #region ReplayButtonWasPressed

        public static event Action<Score> ReplayButtonWasPressedAction;

        public static void NotifyReplayButtonWasPressed(Score score) {
            ReplayButtonWasPressedAction?.Invoke(score);
        }

        #endregion

        #region OnScoreStatsRequested

        public static event Action<int> ScoreStatsRequestedEvent;

        public static void RequestScoreStats(int scoreId) {
            ScoreStatsRequestedEvent?.Invoke(scoreId);
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