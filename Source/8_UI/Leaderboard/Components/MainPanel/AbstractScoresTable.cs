using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
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

        private readonly List<T> _mainRows = new List<T>();

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

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            _root.preferredWidth = RowWidth;
            SetupLayout();

            ScoresRequest.AddStateListener(OnScoresRequestStateChanged);
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibleChanged;
            PluginConfig.LeaderboardTableMaskChangedEvent += OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdateLayout;
            LeaderboardEvents.BattleRoyaleEnabledEvent += OnBattleRoyaleEnabledChanged;
            LeaderboardEvents.ScoreInfoButtonWasPressed += OnScoreClicked;
            LeaderboardState.AddSelectedBeatmapListener(OnBeatmapChanged);

            OnLeaderboardTableMaskChanged(PluginConfig.LeaderboardTableMask);
        }

        protected override void OnDispose() {
            ScoresRequest.RemoveStateListener(OnScoresRequestStateChanged);
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibleChanged;
            PluginConfig.LeaderboardTableMaskChangedEvent -= OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdateLayout;
            LeaderboardEvents.BattleRoyaleEnabledEvent -= OnBattleRoyaleEnabledChanged;
            LeaderboardEvents.ScoreInfoButtonWasPressed -= OnScoreClicked;

            LeaderboardState.RemoveSelectedBeatmapListener(OnBeatmapChanged);
        }

        #endregion

        #region Events

        private void OnBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey beatmapKey, BeatmapLevel beatmapLevel) {
            _selectedContents.Clear();
        }
        
        private void OnScoresRequestStateChanged(API.RequestState state, ScoresTableContent result, string failReason) {
            if (state is not API.RequestState.Finished) {
                PresentContent(null);
                return;
            }

            PresentContent(result);
        }

        private void OnLeaderboardVisibleChanged(bool isVisible) {
            if (isVisible) return;
            StartAnimation();
        }
        
        private void OnBattleRoyaleEnabledChanged(bool brEnabled) {
            _battleRoyaleEnabled = brEnabled;

            if (brEnabled) {
                _selectedContents.Clear();
            }

            RefreshCells();
            StartBattleRoyaleAnimation();
        }
        
        private void OnScoreClicked(Score score) {
            if (!_battleRoyaleEnabled) {
                return;
            }

            if (_selectedContents.Contains(score)) {
                _selectedContents.Remove(score);
            } else {
                _selectedContents.Add(score);
            }

            RefreshCells(true);
        }

        private void OnLeaderboardTableMaskChanged(ScoreRowCellType value) {
            UpdateLayout();
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

        private readonly HashSet<IScoreRowContent> _selectedContents = new();
        private ScoresTableContent? _content;
        private bool _battleRoyaleEnabled;

        public void PresentContent(ScoresTableContent? content) {
            _content = content;

            if (content != null) {
                RefreshCells();
                UpdateLayout();
            } else {
                _extraRow.ClearContent();
                foreach (var scoreRow in _mainRows) {
                    scoreRow.ClearContent();
                }
            }

            StartAnimation();
        }

        private void RefreshCells(bool applyImmediately = false) {
            if (_content == null) {
                return;
            }

            if (_content.ExtraRowContent != null) {
                _extraRow.SetContent(_content.ExtraRowContent);
            }

            for (var i = 0; i < RowsCount; i++) {
                if (i >= _content.MainRowContents.Count) continue;

                var row = _mainRows[i];
                row.SetContent(_content.MainRowContents[i]);

                if (_battleRoyaleEnabled) {
                    var rowSelected = _selectedContents.Contains(_content.MainRowContents[i]);
                    row.SetHighlight(rowSelected);
                }

                if (applyImmediately) {
                    row.ApplyVisualChanges();
                }
            }
        }

        #endregion

        #region Animations

        private ExtraRowState _lastExtraRowState = ExtraRowState.Hidden;
        private const float DelayPerRow = 0.016f;
        
        private void StartBattleRoyaleAnimation() {
            IEnumerator Coroutine() {
                yield return FadeOutCoroutine();
                yield return new WaitForSeconds(0.05f);
                yield return FadeInCoroutine(_content!);
            }

            StartCoroutine(Coroutine());
        }

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

        private IEnumerator FadeInCoroutine(ScoresTableContent content) {
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

        private int BottomSiblingIndex => RowsCount + 2;
        private const int TopSiblingIndex = 0;

        private ExtraRowState UpdateExtraRowState(ScoresTableContent content) {
            if (!_battleRoyaleEnabled && content.ExtraRowContent != null && content.ExtraRowContent.ContainsValue(ScoreRowCellType.Rank)) {
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