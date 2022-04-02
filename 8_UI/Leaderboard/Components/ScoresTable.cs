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

            if (extraRowState == ExtraRowState.Top) {
                UpdateRowLayout(_extraRow.ScoreRow);
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
                UpdateRowLayout(_extraRow.ScoreRow);
                _extraRow.SetScore(scoresData.selection);
            }

            _lastExtraRowState = extraRowState;
        }

        #endregion

        #region ExtraRowUtils

        private void HideExtraRow() {
            _extraRow.SetActive(false);
        }

        private void ShowExtraRow() {
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
    }
}