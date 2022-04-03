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
        private readonly ScoreRow _extraRow = Instantiate<ScoreRow>();

        [UIValue("top-row-divider"), UsedImplicitly]
        private readonly ScoreRowDivider _topRowDivider = Instantiate<ScoreRowDivider>();

        [UIValue("bottom-row-divider"), UsedImplicitly]
        private readonly ScoreRowDivider _bottomRowDivider = Instantiate<ScoreRowDivider>();

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
            if (scoresData.data == null || scoresData.data.IsEmpty()) return;
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

            _topRowDivider.FadeOut();
            _bottomRowDivider.FadeOut();

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
            TableLayoutUtils.CalculateColumns(scoresData,
                out var rankColumnWidth,
                out var accColumnWidth,
                out var ppColumnWidth,
                out var scoreColumnWidth,
                out var flexibleWidth,
                out var hasPP
            );

            void UpdateRowLayout(ScoreRow row) {
                row.UpdateLayout(
                    rankColumnWidth,
                    accColumnWidth,
                    ppColumnWidth,
                    scoreColumnWidth,
                    flexibleWidth,
                    hasPP
                );
            }

            var extraRowState = UpdateExtraRowState(scoresData);

            if (scoresData.metadata.page > 1) _topRowDivider.FadeIn();
            if (scoresData.metadata.page < (float) scoresData.metadata.total / scoresData.metadata.itemsPerPage) _bottomRowDivider.FadeIn();

            if (extraRowState == ExtraRowState.Top) {
                UpdateRowLayout(_extraRow);
                _extraRow.SetScore(scoresData.selection);
                yield return new WaitForSeconds(DelayPerRow);
            }

            for (var i = 0; i < MainRowsCount; i++) {
                var row = _mainRows[i];
                UpdateRowLayout(row);

                if (i < scoresData.data.Count) {
                    row.SetScore(scoresData.data[i]);
                }

                yield return new WaitForSeconds(DelayPerRow);
            }

            if (extraRowState == ExtraRowState.Bottom) {
                UpdateRowLayout(_extraRow);
                _extraRow.SetScore(scoresData.selection);
            }

            _lastExtraRowState = extraRowState;
        }

        #endregion

        #region ExtraRowUtils

        private const int BottomSiblingIndex = MainRowsCount + 2;
        private const int TopSiblingIndex = 0;

        private ExtraRowState UpdateExtraRowState(Paged<Score> scoresData) {
            if (scoresData.selection == null) {
                _extraRow.SetActive(false);
                return ExtraRowState.Hidden;
            }

            if (scoresData.selection.rank < scoresData.data.First().rank) {
                _extraRow.SetHierarchyIndex(TopSiblingIndex);
                _extraRow.SetActive(true);
                return ExtraRowState.Top;
            }

            if (scoresData.selection.rank > scoresData.data.Last().rank) {
                _extraRow.SetHierarchyIndex(BottomSiblingIndex);
                _extraRow.SetActive(true);
                return ExtraRowState.Bottom;
            }

            _extraRow.SetActive(false);
            return ExtraRowState.Hidden;
        }

        private enum ExtraRowState {
            Top,
            Bottom,
            Hidden
        }

        #endregion
    }
}