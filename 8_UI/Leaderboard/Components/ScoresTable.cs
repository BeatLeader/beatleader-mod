using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoresTable.bsml")]
    internal class ScoresTable : ReeUIComponent {
        #region Components

        [UIValue("extra-score-row"), UsedImplicitly]
        private readonly ExtraScoreRow _extraRow = Instantiate<ExtraScoreRow>(it => it.SetActive(false));

        [UIValue("score-rows"), UsedImplicitly]
        private readonly List<object> _scoreRowsObj = new();

        private readonly List<ScoreRow> _mainRows = new();

        #endregion

        #region Constructor

        private const int MainRowsCount = 10;

        public ScoresTable() {
            for (var i = 0; i < MainRowsCount; i++) {
                var scoreRow = Instantiate<ScoreRow>();
                _scoreRowsObj.Add(scoreRow);
                _mainRows.Add(scoreRow);
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
            StartCoroutine(ClearScoresCoroutine());
        }

        private void OnScoresFetched(Paged<Score> scoresData) {
            if (scoresData.data == null) {
                Plugin.Log.Error("scoresData.data is null!");
                return;
            }

            if (scoresData.data.IsEmpty()) return; //TODO: Display "You can be the first!" message
            StartCoroutine(SetScoresCoroutine(scoresData));
        }

        #endregion

        #region Animations

        private ExtraRowState _lastExtraRowState = ExtraRowState.Hidden;
        private const float DelayPerRow = 0.016f;

        private IEnumerator ClearScoresCoroutine() {
            if (_lastExtraRowState == ExtraRowState.Top) {
                _extraRow.ClearScore();
                yield return new WaitForSeconds(DelayPerRow);
            }

            foreach (var row in _mainRows) {
                row.ClearScore();
                yield return new WaitForSeconds(DelayPerRow);
            }

            if (_lastExtraRowState == ExtraRowState.Bottom) {
                _extraRow.ClearScore();
                yield return new WaitForSeconds(DelayPerRow);
            }

            _lastExtraRowState = ExtraRowState.Hidden;
        }

        private IEnumerator SetScoresCoroutine(Paged<Score> scoresData) {
            CalculateColumns(scoresData,
                out var rankColumnWidth,
                out var nameColumnWidth,
                out var accColumnWidth,
                out var ppColumnWidth,
                out var scoreColumnWidth,
                out var infoColumnWidth,
                out var hasPP
            );

            void UpdateRowLayout(ScoreRow row) {
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

            var extraRowState = UpdateExtraRowState(scoresData);

            if (extraRowState == ExtraRowState.Top) {
                _extraRow.SetScore(scoresData.selection);
                UpdateRowLayout(_extraRow.ScoreRow);
                yield return new WaitForSeconds(DelayPerRow);
            }

            for (var i = 0; i < MainRowsCount; i++) {
                var row = _mainRows[i];

                if (i < scoresData.data.Count) {
                    row.SetScore(scoresData.data[i]);
                }

                UpdateRowLayout(row);
                yield return new WaitForSeconds(DelayPerRow);
            }

            if (extraRowState == ExtraRowState.Bottom) {
                _extraRow.SetScore(scoresData.selection);
                UpdateRowLayout(_extraRow.ScoreRow);
            }

            _lastExtraRowState = extraRowState;
        }

        #endregion

        #region ExtraRowUtils

        private const int PaddingWithExtraRow = 0;
        private const int PaddingWithoutExtraRow = 4;

        private void HideExtraRow() {
            VerticalPadding = PaddingWithoutExtraRow;
            _extraRow.SetActive(false);
        }

        private void ShowExtraRow() {
            VerticalPadding = PaddingWithExtraRow;
            _extraRow.SetActive(true);
        }

        private ExtraRowState UpdateExtraRowState(Paged<Score> scoresData) {
            if (scoresData.selection == null) {
                HideExtraRow();
                return ExtraRowState.Hidden;
            }

            if (scoresData.selection.rank < scoresData.data.First().rank) {
                ShowExtraRow();
                _extraRow.SetHierarchyIndex(0);
                _extraRow.SetRowPosition(true);
                return ExtraRowState.Top;
            }

            if (scoresData.selection.rank > scoresData.data.Last().rank) {
                ShowExtraRow();
                _extraRow.SetHierarchyIndex(MainRowsCount + 1);
                _extraRow.SetRowPosition(false);
                return ExtraRowState.Bottom;
            }

            HideExtraRow();
            return ExtraRowState.Hidden;
        }

        private enum ExtraRowState {
            Top,
            Bottom,
            Hidden
        }

        #endregion

        #region RecalculateTableLayout //TODO: Make automatic 

        private const float TotalWidth = 85.0f;
        private const float Spacing = 1.0f;
        private const float Pad = 2.0f;
        private const int ColumnsCount = 6;

        private const float RankMinWidth = 3.0f;
        private const float AccMinWidth = 9.0f;
        private const float PPMinWidth = 9.0f;
        private const float ScoreMinWidth = 3.0f;
        private const float InfoMinWidth = 5.0f;

        private const float ApproxCharacterWidth = 1.2f;

        private static void CalculateColumns(Paged<Score> scoresData,
            out float rankColumnWidth,
            out float nameColumnWidth,
            out float accColumnWidth,
            out float ppColumnWidth,
            out float scoreColumnWidth,
            out float infoColumnWidth,
            out bool hasPP
        ) {
            var maximalRank = 0;
            var maximalScore = 0;
            hasPP = false;

            if (scoresData.selection != null) {
                if (scoresData.selection.pp > 0) hasPP = true;
                if (scoresData.selection.baseScore > maximalScore) maximalScore = scoresData.selection.baseScore;
                if (scoresData.selection.rank > maximalRank) maximalRank = scoresData.selection.rank;
            }

            foreach (var score in scoresData.data) {
                if (score.pp > 0) hasPP = true;
                if (score.baseScore > maximalScore) maximalScore = score.baseScore;
                if (score.rank > maximalRank) maximalRank = score.rank;
            }

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

        #region VerticalPadding

        private int _verticalPadding;

        [UIValue("vertical-padding"), UsedImplicitly]
        public int VerticalPadding {
            get => _verticalPadding;
            set {
                if (_verticalPadding.Equals(value)) return;
                _verticalPadding = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}