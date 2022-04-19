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

        #region ContextWasSelected

        public static event Action<ScoresContext> ContextWasSelectedAction;

        public static void NotifyContextWasSelected(ScoresContext context) {
            ContextWasSelectedAction?.Invoke(context);
        }

        #endregion

        #region ReplayButtonWasPressed

        public static event Action<Score> ReplayButtonWasPressedAction;

        public static void NotifyReplayButtonWasPressed(Score score) {
            ReplayButtonWasPressedAction?.Invoke(score);
        }

        #endregion

        //-- INTERNAL -----------------

        #region ScoreInfoButtonWasPressed

        public static event Action<Score> ScoreInfoButtonWasPressed;

        public static void NotifyScoreInfoButtonWasPressed(Score score) {
            ScoreInfoButtonWasPressed?.Invoke(score);
        }

        #endregion

        #region ScoreInfoPanelTabWasSelected

        public static event Action<ScoreInfoPanelTab> ScoreInfoPanelTabWasSelected;

        public static void NotifyScoreInfoPanelTabWasSelected(ScoreInfoPanelTab tab) {
            ScoreInfoPanelTabWasSelected?.Invoke(tab);
        }

        #endregion
    }
}