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

        protected override void OnInitialize() {
            SetupLayout();

            PluginConfig.LeaderboardTableMaskChangedEvent += OnLeaderboardTableMaskChanged;
            LeaderboardState.ScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            OnLeaderboardTableMaskChanged(PluginConfig.LeaderboardTableMask);
            OnScoresRequestStateChanged(LeaderboardState.ScoresRequest.State);
        }

        protected override void OnDispose() {
            PluginConfig.LeaderboardTableMaskChangedEvent -= OnLeaderboardTableMaskChanged;
            LeaderboardState.ScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
        }

        #endregion

        #region Events

        private void OnLeaderboardTableMaskChanged(ScoreRowCellType value) {
            UpdateLayout();
        }

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
            ClearScoresValues();
            if (gameObject.activeInHierarchy) {
                StartCoroutine(FadeOutCoroutine());
            } else {
                FadeOutInstant();
            }
        }

        private void ShowScores(Paged<Score> scoresData) {
            if (scoresData.data == null || scoresData.data.IsEmpty()) return;
            SetScoresValues(scoresData);
            if (gameObject.activeInHierarchy) {
                StartCoroutine(FadeInCoroutine(scoresData));
            } else {
                FadeInInstant(scoresData);
            }
        }

        #endregion

        #region Layout

        private readonly ScoresTableLayoutHelper _layoutHelper = new();
        private bool _hasPP;

        private void SetupLayout() {
            _extraRow.SetupLayout(_layoutHelper);
            foreach (var scoreRow in _mainRows) {
                scoreRow.SetupLayout(_layoutHelper);
            }
        }

        private void UpdateLayout() {
            _layoutHelper.RecalculateLayout(PluginConfig.GetLeaderboardTableMask(_hasPP));
        }

        #endregion

        #region SetScoresValues

        private void ClearScoresValues() {
            _extraRow.ClearScore();
            foreach (var scoreRow in _mainRows) {
                scoreRow.ClearScore();
            }
        }

        private void SetScoresValues(Paged<Score> scoresData) {
            if (scoresData.selection != null) _extraRow.SetScore(scoresData.selection);
            _hasPP = false;
            for (var i = 0; i < MainRowsCount; i++) {
                if (i >= scoresData.data.Count) continue;
                if (scoresData.data[i].pp > 0) _hasPP = true;
                _mainRows[i].SetScore(scoresData.data[i]);
            }
            UpdateLayout();
        }

        #endregion

        #region Animations

        private ExtraRowState _lastExtraRowState = ExtraRowState.Hidden;
        private const float DelayPerRow = 0.016f;

        private IEnumerator FadeOutCoroutine() {
            if (_lastExtraRowState == ExtraRowState.Top) {
                _extraRow.FadeOut();
                yield return new WaitForSeconds(DelayPerRow);
            }

            _topRowDivider.FadeOut();

            foreach (var row in _mainRows) {
                row.FadeOut();
                yield return new WaitForSeconds(DelayPerRow);
            }
            
            _bottomRowDivider.FadeOut();

            if (_lastExtraRowState == ExtraRowState.Bottom) {
                _extraRow.FadeOut();
                yield return new WaitForSeconds(DelayPerRow);
            }

            _lastExtraRowState = ExtraRowState.Hidden;
        }

        private IEnumerator FadeInCoroutine(Paged<Score> scoresData) {
            var extraRowState = UpdateExtraRowState(scoresData);
            
            if (extraRowState == ExtraRowState.Top) {
                _extraRow.FadeIn();
                yield return new WaitForSeconds(DelayPerRow);
            }
            
            if (scoresData.metadata.page > 1) _topRowDivider.FadeIn();

            for (var i = 0; i < MainRowsCount; i++) {
                var row = _mainRows[i];
                if (i < scoresData.data.Count) row.FadeIn();
                yield return new WaitForSeconds(DelayPerRow);
            }

            if (scoresData.metadata.page < (float) scoresData.metadata.total / scoresData.metadata.itemsPerPage) _bottomRowDivider.FadeIn();
            
            if (extraRowState == ExtraRowState.Bottom) {
                _extraRow.FadeIn();
            }

            _lastExtraRowState = extraRowState;
        }

        #endregion

        #region Instant

        private void FadeOutInstant() {
            _extraRow.FadeOut();
            _topRowDivider.FadeOut();
            _bottomRowDivider.FadeOut();
            foreach (var row in _mainRows) {
                row.FadeOut();
            }
            _lastExtraRowState = ExtraRowState.Hidden;
        }

        private void FadeInInstant(Paged<Score> scoresData) {
            var extraRowState = UpdateExtraRowState(scoresData);

            _extraRow.FadeIn();
            if (scoresData.metadata.page > 1) _topRowDivider.FadeIn();
            if (scoresData.metadata.page < (float) scoresData.metadata.total / scoresData.metadata.itemsPerPage) _bottomRowDivider.FadeIn();

            for (var i = 0; i < MainRowsCount; i++) {
                var row = _mainRows[i];
                if (i < scoresData.data.Count) row.FadeIn();
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