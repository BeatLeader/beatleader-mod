using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using ModestTree;

namespace BeatLeader {
    internal partial class LeaderboardView {
        #region Init/Dispose

        private void InitializeScoresSection() {
            _leaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarter;
            _leaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
            UpdateTableLayout(10, 1_000_000, true);
        }

        private void DisposeScoresSection() {
            _leaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarter;
            _leaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
        }

        #endregion

        #region Events

        private void OnInfoButtonClicked(int index) {
            //TODO: Open Modal
            Plugin.Log.Info($"Info button clicked: {index}");
        }

        private void OnScoreRequestStarter() {
            //TODO: Spinner
            for (var i = 0; i < 10; i++) {
                ClearScore(i);
            }
        }

        private void OnScoresFetched(List<Score> scores) {
            if (scores.IsEmpty()) {
                for (var i = 0; i < 10; i++) {
                    ClearScore(i);
                }

                UpdateTableLayout(0, 0, false);
            } else {
                var hasPP = false;

                for (var i = 0; i < 10; i++) {
                    if (i < scores.Count) {
                        SetScore(i, scores[i]);
                        if (scores[i].pp > 0) hasPP = true;
                    } else {
                        ClearScore(i);
                    }
                }

                UpdateTableLayout(scores.Last().rank, scores.First().baseScore, hasPP);
            }
        }

        #endregion
    }
}