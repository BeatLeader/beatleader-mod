using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader {
    internal partial class LeaderboardView {
        #region Init/Dispose

        private void InitializeScoresSection() {
            _leaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarter;
            _leaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
        }

        private void DisposeScoresSection() {
            _leaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarter;
            _leaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
        }

        #endregion

        #region Events

        private void OnScoreRequestStarter() {
            PlaceholderText = "Loading...";
        }

        private void OnScoresFetched(List<Score> scores) {
            Plugin.Log.Debug("Processing scores for UI");
            if (scores.Count > 0) {
                var txt = "";
                scores.ForEach(score => {
                    //txt += $"#{score.rank}\t{score.player.name}\t\t{score.pp}pp\t{score.baseScore} ({(int)(score.accuracy*10000) / 100}%)\n";
                    txt += String.Format("#{0,-3} {1,-20} {2,6:.00}pp {3,-10} ({4:.00}%) \n", score.rank, score.player.name, score.pp, score.baseScore,
                        score.accuracy * 100);
                });
                Plugin.Log.Debug("Updating score panel");
                Plugin.Log.Debug(txt);
                PlaceholderText = txt;
            } else {
                PlaceholderText = "No scores found";
            }
        }

        #endregion

        #region PlaceholderText

        [UsedImplicitly] private string _placeholderText = "Such scores!";

        [UIValue("placeholder-text")]
        public string PlaceholderText {
            get { return _placeholderText; }
            set {
                if (_placeholderText.Equals(value)) return;
                _placeholderText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}