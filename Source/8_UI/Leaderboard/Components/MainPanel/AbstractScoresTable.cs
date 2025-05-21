using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.WebRequests;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class AbstractScoresTable<T> : ReeUIComponentV2 where T : AbstractScoreRow {
        #region Properties

        protected abstract int RowsCount { get; }
        protected abstract float RowWidth { get; }
        protected abstract float Spacing { get; }
        protected abstract ScoreRowCellType CellTypeMask { get; }

        #endregion

        #region Components

        [UIComponent("Root"), UsedImplicitly]
        private protected LayoutElement _root;

        [UIValue("extra-score-row"), UsedImplicitly]
        private protected T _extraRow;

        [UIValue("top-row-divider"), UsedImplicitly]
        private protected ScoreRowDivider _topRowDivider;

        [UIValue("bottom-row-divider"), UsedImplicitly]
        private protected ScoreRowDivider _bottomRowDivider;

        [UIValue("score-rows"), UsedImplicitly]
        private protected readonly List<object> _scoreRowsObj = new List<object>();

        protected readonly List<T> _mainRows = new List<T>();

        private ScoresTableLayoutHelper _layoutHelper;

        private void Awake() {
            _layoutHelper = new ScoresTableLayoutHelper(RowWidth, Spacing);

            _extraRow = Instantiate<T>(transform);
            _topRowDivider = Instantiate<ScoreRowDivider>(transform);
            _bottomRowDivider = Instantiate<ScoreRowDivider>(transform);

            for (var i = 0; i < RowsCount; i++) {
                var scoreRow = Instantiate<T>(transform);
                _scoreRowsObj.Add(scoreRow);
                _mainRows.Add(scoreRow);
            }
        }

        protected override void OnInitialize() {
            _root.preferredWidth = RowWidth;
            SetupLayout();
        }

        #endregion

        #region Layout

        private void SetupLayout() {
            _extraRow.SetupLayout(_layoutHelper);
            foreach (var scoreRow in _mainRows) {
                scoreRow.SetupLayout(_layoutHelper);
            }
        }

        protected void UpdateLayout() {
            var mask = CellTypeMask;

            if (_content != null) {
                if (_content.ForceClanTags) mask |= ScoreRowCellType.Clans;

                foreach (var item in Enum.GetValues(typeof(ScoreRowCellType))) {
                    var cellType = (ScoreRowCellType)item;
                    if (!mask.HasFlag(cellType)) continue;

                    var isCellPresent = false;
                    Check(_content.ExtraRowContent);
                    foreach (var rowContent in _content.MainRowContents) {
                        Check(rowContent);
                    }

                    if (!isCellPresent) mask &= ~cellType;

                    continue;

                    void Check(IScoreRowContent? rowContent) {
                        if (isCellPresent || rowContent == null || !rowContent.ContainsValue(cellType)) return;
                        isCellPresent = true;
                    }
                }
            }

            _layoutHelper.RecalculateLayout(mask);
        }

        #endregion

        #region Content

        protected ScoresTableContent? _content;

        public void PresentContent(ScoresTableContent? content) {
            _content = content;

            if (content != null) {
                if (content.ExtraRowContent != null) _extraRow.SetContent(content.ExtraRowContent);

                for (var i = 0; i < RowsCount; i++) {
                    if (i >= content.MainRowContents.Count) continue;
                    _mainRows[i].SetContent(content.MainRowContents[i]);
                }

                UpdateLayout();
            } else {
                _extraRow.ClearContent();
                foreach (var scoreRow in _mainRows) {
                    scoreRow.ClearContent();
                }
            }

            StartAnimation();
        }

        #endregion

        #region Animations

        private ExtraRowState _lastExtraRowState = ExtraRowState.Hidden;
        private const float DelayPerRow = 0.016f;

        protected void StartAnimation() {
            if (gameObject.activeInHierarchy) {
                StartCoroutine(_content == null ? FadeOutCoroutine() : FadeInCoroutine(_content));
            } else {
                if (_content == null) {
                    FadeOutInstant();
                } else {
                    FadeInInstant(_content);
                }
            }
        }

        protected IEnumerator FadeOutCoroutine() {
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

        protected IEnumerator FadeInCoroutine(ScoresTableContent content) {
            var extraRowState = UpdateExtraRowState(content);

            if (extraRowState == ExtraRowState.Top) {
                _extraRow.FadeIn();
                yield return new WaitForSeconds(DelayPerRow);
            }

            if (content.CurrentPage > 1) {
                _topRowDivider.FadeIn();
            }

            for (var i = 0; i < RowsCount; i++) {
                var row = _mainRows[i];
                if (i < content.MainRowContents.Count) row.FadeIn();
                yield return new WaitForSeconds(DelayPerRow);
            }

            if (content.CurrentPage < content.PagesCount) {
                _bottomRowDivider.FadeIn();
            }

            if (extraRowState == ExtraRowState.Bottom) {
                _extraRow.FadeIn();
            }

            _lastExtraRowState = extraRowState;
        }

        private void FadeOutInstant() {
            _extraRow.FadeOut();
            _topRowDivider.FadeOut();
            _bottomRowDivider.FadeOut();
            foreach (var row in _mainRows) {
                row.FadeOut();
            }

            _lastExtraRowState = ExtraRowState.Hidden;
        }

        private void FadeInInstant(ScoresTableContent content) {
            var extraRowState = UpdateExtraRowState(content);

            _extraRow.FadeIn();
            if (content.CurrentPage > 1) _topRowDivider.FadeIn();
            if (content.CurrentPage < content.PagesCount) _bottomRowDivider.FadeIn();

            for (var i = 0; i < RowsCount; i++) {
                var row = _mainRows[i];
                if (i < content.MainRowContents.Count) row.FadeIn();
            }

            _lastExtraRowState = extraRowState;
        }

        #endregion

        #region ExtraRowUtils

        protected virtual bool AllowExtraRow => true;

        private int BottomSiblingIndex => RowsCount + 2;
        private const int TopSiblingIndex = 0;

        private ExtraRowState UpdateExtraRowState(ScoresTableContent content) {
            if (AllowExtraRow && content.ExtraRowContent != null && content.ExtraRowContent.ContainsValue(ScoreRowCellType.Rank)) {
                var extraRowRank = (int)(content.ExtraRowContent.GetValue(ScoreRowCellType.Rank) ?? 0);

                var firstRowRank = (int)(content.MainRowContents.First()?.GetValue(ScoreRowCellType.Rank) ?? 0);
                if (extraRowRank < firstRowRank) {
                    _extraRow.SetHierarchyIndex(TopSiblingIndex);
                    _extraRow.SetActive(true);
                    return ExtraRowState.Top;
                }

                var lastRowRank = (int)(content.MainRowContents.Last()?.GetValue(ScoreRowCellType.Rank) ?? 0);
                if (extraRowRank > lastRowRank) {
                    _extraRow.SetHierarchyIndex(BottomSiblingIndex);
                    _extraRow.SetActive(true);
                    return ExtraRowState.Bottom;
                }
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