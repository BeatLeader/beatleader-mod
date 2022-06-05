using System;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class LeaderboardState {
        #region Requests

        public static readonly RequestStateHandler<Player> ProfileRequest = new();
        public static readonly RequestStateHandler<Score> UploadRequest = new();
        public static readonly RequestStateHandler<Paged<Score>> ScoresRequest = new();
        public static readonly RequestStateHandler<ScoreStats> ScoreStatsRequest = new();
        public static readonly RequestStateHandler<VoteStatus> VoteStatusRequest = new();
        public static readonly RequestStateHandler<VoteStatus> VoteRequest = new();

        #endregion

        #region IsVisible

        public static event Action<bool> IsVisibleChangedEvent;

        private static bool _isVisible;

        public static bool IsVisible {
            get => _isVisible;
            set {
                if (_isVisible.Equals(value)) return;
                _isVisible = value;
                IsVisibleChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region ScoresScope

        public static event Action<ScoresScope> ScoresScopeChangedEvent;

        private static ScoresScope _scoresScope = ScoresScope.Global;

        public static ScoresScope ScoresScope {
            get => _scoresScope;
            set {
                if (_scoresScope.Equals(value)) return;
                _scoresScope = value;
                ScoresScopeChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region ScoreInfoPanelTab

        public static event Action<ScoreInfoPanelTab> ScoreInfoPanelTabChangedEvent;

        private static ScoreInfoPanelTab _scoreInfoPanelTab = ScoreInfoPanelTab.Overview;

        public static ScoreInfoPanelTab ScoreInfoPanelTab {
            get => _scoreInfoPanelTab;
            set {
                if (_scoreInfoPanelTab.Equals(value)) return;
                _scoreInfoPanelTab = value;
                ScoreInfoPanelTabChangedEvent?.Invoke(value);
            }
        }

        #endregion
    }
}