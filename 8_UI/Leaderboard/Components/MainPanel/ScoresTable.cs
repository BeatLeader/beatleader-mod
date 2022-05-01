using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ScoresTable : ReeUIComponentV2 {
        #region Components

        private const int MainRowsCount = 10;

        [UIValue("extra-score-row"), UsedImplicitly]
        private ScoreRow _extraRow;

        [UIValue("top-row-divider"), UsedImplicitly]
        private ScoreRowDivider _topRowDivider;

        [UIValue("bottom-row-divider"), UsedImplicitly]
        private ScoreRowDivider _bottomRowDivider;

        [UIValue("score-rows"), UsedImplicitly]
        private readonly List<object> _scoreRowsObj = new();

        private readonly List<ScoreRow> _mainRows = new();

        private void Awake() {
            _extraRow = Instantiate<ScoreRow>(transform);
            _topRowDivider = Instantiate<ScoreRowDivider>(transform);
            _bottomRowDivider = Instantiate<ScoreRowDivider>(transform);
            
            for (var i = 0; i < MainRowsCount; i++) {
                var scoreRow = Instantiate<ScoreRow>(transform);
                _scoreRowsObj.Add(scoreRow);
                _mainRows.Add(scoreRow);
            }
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnAfterParse() {
            LeaderboardState.ScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            OnScoresRequestStateChanged(LeaderboardState.ScoresRequest.State);
        }

        protected override void OnDispose() {
            LeaderboardState.ScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
        }

        #endregion

        #region Events

        private void OnScoresRequestStateChanged(RequestState requestState) {
            switch (requestState) {
                case RequestState.Uninitialized:
                case RequestState.Started:
                case RequestState.Failed:
                    ClearScores();
                    break;
                case RequestState.Finished:
                    ShowScores(LeaderboardState.ScoresRequest.Result);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(requestState), requestState, null);
            }
        }

        private void ClearScores() {
            if (gameObject.activeInHierarchy) {
                StartCoroutine(ClearScoresCoroutine());
            } else {
                ClearScoresInstant();
            }
        }

        private void ShowScores(Paged<Score> scoresData) {
            if (scoresData.data == null || scoresData.data.IsEmpty()) return;
            if (gameObject.activeInHierarchy) {
                StartCoroutine(SetScoresCoroutine(scoresData));
            } else {
                SetScoresInstant(scoresData);
            }
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

        #region Instant

        private void ClearScoresInstant() {
            if (_lastExtraRowState == ExtraRowState.Top) {
                _extraRow.ClearScore();
            }

            _topRowDivider.FadeOut();
            _bottomRowDivider.FadeOut();

            foreach (var row in _mainRows) {
                row.ClearScore();
            }

            if (_lastExtraRowState == ExtraRowState.Bottom) {
                _extraRow.ClearScore();
            }

            _lastExtraRowState = ExtraRowState.Hidden;
        }

        private void SetScoresInstant(Paged<Score> scoresData) {
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
            }

            for (var i = 0; i < MainRowsCount; i++) {
                var row = _mainRows[i];
                UpdateRowLayout(row);

                if (i < scoresData.data.Count) {
                    row.SetScore(scoresData.data[i]);
                }
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