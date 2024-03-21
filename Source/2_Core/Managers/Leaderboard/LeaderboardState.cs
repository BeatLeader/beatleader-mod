using System;
using BeatLeader.Models;
using JetBrains.Annotations;

namespace BeatLeader {
    internal static class LeaderboardState {
        #region SelectedBeatmap

        public delegate void SelectedBeatmapWasChangedDelegate(
            bool selectedAny, 
            LeaderboardKey leaderboardKey, 
            [CanBeNull] BeatmapKey beatmapKey,
            [CanBeNull] BeatmapLevel beatmapLevel);

        private static event SelectedBeatmapWasChangedDelegate SelectedBeatmapWasChangedEvent;

        [CanBeNull]
        private static BeatmapKey _selectedBeatmapKey;
        [CanBeNull]
        private static BeatmapLevel _selectedBeatmapLevel;

        public static LeaderboardKey SelectedLeaderboardKey { get; private set; }
        public static bool IsAnyBeatmapSelected;

        [CanBeNull]
        public static BeatmapKey SelectedBeatmapKey {
            get => _selectedBeatmapKey;
            set {
                if (_selectedBeatmapKey == value) return;
                _selectedBeatmapKey = value;
                IsAnyBeatmapSelected = value != null;
                SelectedLeaderboardKey = LeaderboardKey.FromBeatmap(value);
                SelectedBeatmapWasChangedEvent?.Invoke(IsAnyBeatmapSelected, SelectedLeaderboardKey, value, _selectedBeatmapLevel);
            }
        }

        [CanBeNull]
        public static BeatmapLevel SelectedBeatmapLevel {
            get => _selectedBeatmapLevel;
            set {
                if (_selectedBeatmapLevel == value) return;
                _selectedBeatmapLevel = value;
            }
        }

        public static void AddSelectedBeatmapListener(SelectedBeatmapWasChangedDelegate handler) {
            SelectedBeatmapWasChangedEvent += handler;
            handler?.Invoke(IsAnyBeatmapSelected, SelectedLeaderboardKey, SelectedBeatmapKey, SelectedBeatmapLevel);
        }

        public static void RemoveSelectedBeatmapListener(SelectedBeatmapWasChangedDelegate handler) {
            SelectedBeatmapWasChangedEvent -= handler;
        }

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

        private static ScoreInfoPanelTab _scoreInfoPanelTab = ScoreInfoPanelTab.OverviewPage1;

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