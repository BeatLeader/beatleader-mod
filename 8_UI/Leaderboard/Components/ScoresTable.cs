using System.Collections.Generic;
using System.Linq;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoresTable.bsml")]
    internal class ScoresTable : ReeUIComponent {
        #region Constructor

        private const int RowsCount = 10;

        public ScoresTable() {
            for (var i = 0; i < RowsCount; i++) {
                var scoreRow = Instantiate<ScoreRow>();
                _scoreRowsObj.Add(scoreRow);
                _scoreRows.Add(scoreRow);
            }
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
        }

        protected override void OnDispose() {
            LeaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
        }

        #endregion

        #region Events

        private void OnScoreRequestStarted() {
            //TODO: Spinner
            for (var i = 0; i < 10; i++) {
                ClearScore(i);
            }
        }

        private void OnScoresFetched(Paged<List<Score>> scoresData) {
            var scores = scoresData.data;

            var maximalRank = 0;
            var maximalScore = 0;
            var hasPP = false;

            for (var i = 0; i < _scoreRows.Count; i++) {
                if (i < scores.Count) {
                    var score = scores[i];
                    if (score.pp > 0) hasPP = true;
                    if (score.rank > maximalRank) maximalRank = score.rank;
                    if (score.baseScore > maximalScore) maximalScore = score.baseScore;
                    SetScore(i, score);
                } else {
                    ClearScore(i);
                }
            }

            UpdateTableLayout(scores.Last().rank, scores.First().baseScore, hasPP);
        }

        #endregion

        #region Rows

        [UIValue("score-rows"), UsedImplicitly]
        private readonly List<object> _scoreRowsObj = new();

        private readonly List<ScoreRow> _scoreRows = new();

        #endregion

        #region SetScores

        private void ClearScores() {
            foreach (var row in _scoreRows) {
                row.ClearScore();
            }

            UpdateTableLayout(0, 0, false);
        }

        private void SetScores(IReadOnlyList<Score> scores, [CanBeNull] Score playerScore) {
            var maximalRank = 0;
            var maximalScore = 0;
            var hasPP = false;

            for (var i = 0; i < RowsCount; i++) {
                if (i < scores.Count) {
                    var score = scores[i];
                    SetScore(i, score);
                    if (score.pp > 0) hasPP = true;
                    if (score.baseScore > maximalScore) maximalScore = score.baseScore;
                    if (score.rank > maximalRank) maximalRank = score.rank;
                } else ClearScore(i);
            }

            UpdateTableLayout(maximalRank, maximalScore, hasPP);
        }

        private void SetScore(int rowIndex, Score score) {
            _scoreRows[rowIndex].SetScore(score, rowIndex == 4);
        }

        private void ClearScore(int rowIndex) {
            _scoreRows[rowIndex].ClearScore();
        }

        #endregion

        #region RecalculateTableLayout //TODO: Make automatic 

        private const float TotalWidth = 85.0f;
        private const float Spacing = 1.0f;
        private const float Pad = 2.0f;
        private const int ColumnsCount = 6;

        private const float RankMinWidth = 3.0f;
        private const float AccMinWidth = 11.0f;
        private const float PPMinWidth = 11.0f;
        private const float ScoreMinWidth = 10.0f;
        private const float InfoMinWidth = 7.0f;

        private const float ApproxCharacterWidth = 1.5f;

        private void UpdateTableLayout(int maximalRank, int maximalScore, bool hasPP) {
            CalculateColumns(maximalRank, maximalScore, hasPP,
                out var rankColumnWidth,
                out var nameColumnWidth,
                out var accColumnWidth,
                out var ppColumnWidth,
                out var scoreColumnWidth,
                out var infoColumnWidth
            );

            foreach (var row in _scoreRows) {
                row.UpdateLayout(
                    rankColumnWidth,
                    nameColumnWidth,
                    accColumnWidth,
                    ppColumnWidth,
                    scoreColumnWidth,
                    infoColumnWidth,
                    hasPP
                );
            }
        }

        private static void CalculateColumns(int maximalRank, int maximalScore, bool hasPP,
            out float rankColumnWidth,
            out float nameColumnWidth,
            out float accColumnWidth,
            out float ppColumnWidth,
            out float scoreColumnWidth,
            out float infoColumnWidth
        ) {
            rankColumnWidth = CalculateRankWidth(maximalRank);
            accColumnWidth = AccMinWidth;
            ppColumnWidth = PPMinWidth;
            scoreColumnWidth = CalculateScoreWidth(maximalScore);
            infoColumnWidth = InfoMinWidth;
            nameColumnWidth = CalculateNameWidth(rankColumnWidth, accColumnWidth, ppColumnWidth, scoreColumnWidth, infoColumnWidth, hasPP);
        }

        private static float CalculateRankWidth(int maximalRank) {
            var charCount = maximalRank.ToString().Length;
            return Mathf.Max(RankMinWidth, ApproxCharacterWidth * charCount);
        }

        private static float CalculateScoreWidth(int maximalScore) {
            var charCount = maximalScore.ToString("N0", ScoreRow.ScoreFormatInfo).Length;
            return Mathf.Max(ScoreMinWidth, ApproxCharacterWidth * charCount);
        }

        private static float CalculateNameWidth(
            float rankColumnWidth,
            float accColumnWidth,
            float ppColumnWidth,
            float scoreColumnWidth,
            float infoColumnWidth,
            bool hasPP
        ) {
            var result = TotalWidth - Pad * 2;

            if (hasPP) {
                result -= ppColumnWidth;
                result -= Spacing * (ColumnsCount - 1);
            } else {
                result -= Spacing * (ColumnsCount - 2);
            }

            result -= rankColumnWidth;
            result -= scoreColumnWidth;
            result -= accColumnWidth;
            result -= infoColumnWidth;

            return result;
        }

        #endregion
    }
}