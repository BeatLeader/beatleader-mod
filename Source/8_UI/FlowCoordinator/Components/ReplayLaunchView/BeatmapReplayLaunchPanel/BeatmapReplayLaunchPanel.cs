using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapReplayLaunchPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("replays-list"), UsedImplicitly]
        private ReplaysList _replaysList = null!;

        [UIValue("replays-list-settings-panel"), UsedImplicitly]
        private ReplaysListSettingsPanel _replaysListSettingsPanel = null!;

        [UIValue("replay-panel"), UsedImplicitly]
        private ReplayDetailPanel _replayPanel = null!;

        [UIComponent("loading-container")]
        private readonly ImageView _loadingContainerBackground = null!;

        [UIObject("loading-container")]
        private readonly GameObject _loadingContainerObject = null!;

        [UIObject("main-container")]
        private readonly GameObject _mainContainerObject = null!;

        [UIObject("settings-panel-container")]
        private readonly GameObject _settingsPanelContainerObject = null!;

        private CanvasGroup _mainContainerCanvasGroup = null!;

        #endregion

        #region Init

        private IReplayManager _replayManager = null!;
        private bool _isInitialized;

        public void Setup(IReplayManager replayManager, ReplayerMenuLoader loader) {
            _replayManager = replayManager;
            _replayManager.ReplaysDeletedEvent += HandleReplaysDeleted;
            _replayManager.ReplayDeletedEvent += HandleReplayDeleted;
            _replayManager.ReplayAddedEvent += HandleReplayAdded;
            _replayPanel.Setup(loader);
            _replayPanel.SetData(null);
            _isInitialized = true;
        }

        protected override void OnInitialize() {
            _loadingContainerBackground.sprite = BundleLoader.TransparentPixel;
            _loadingContainerBackground.color = Color.clear;
            _mainContainerCanvasGroup = _mainContainerObject.AddComponent<CanvasGroup>();
            //_mainContainerCanvasGroup.ignoreParentGroups = true;
        }

        protected override void OnInstantiate() {
            _replaysList = Instantiate<ReplaysList>(transform);
            _replaysListSettingsPanel = Instantiate<ReplaysListSettingsPanel>(transform);
            _replayPanel = Instantiate<ReplayDetailPanel>(transform);
            _replaysList.ReplaySelectedEvent += HandleReplaySelected;
            _replaysList.ShowEmptyScreenChangedEvent += HandleShowEmptyScreenChanged;
            _replaysListSettingsPanel.SorterChangedEvent += HandleSorterChanged;
            _replaysListSettingsPanel.ShowCorruptedChangedEvent += HandleShowCorruptedChangedEvent;
        }

        protected override void OnDispose() {
            _replayManager.ReplaysDeletedEvent -= HandleReplaysDeleted;
            _replayManager.ReplayDeletedEvent -= HandleReplayDeleted;
            _replayManager.ReplayAddedEvent -= HandleReplayAdded;
        }

        #endregion

        #region ReplayHeaders

        private IEnumerable<IReplayHeader>? _headers;
        private IEnumerable<IReplayHeader>? _tempHeaders;
        private CancellationTokenSource? _tokenSource;

        private void CancelReplayInfosLoading() {
            if (_tokenSource == null) return;
            _tokenSource.Cancel();
            FinishReplayLoading(true);
        }

        private async Task StartReplayInfosLoading(string? levelId, SynchronizationContext context) {
            if (_tokenSource != null) return;
            _replaysList.SetData();
            ShowLoadingScreen(true);
            _tokenSource = new();
            var token = _tokenSource.Token;
            _tempHeaders = await _replayManager.LoadReplayHeadersAsync(token, x => {
                if (x.FileStatus is not FileStatus.Corrupted
                    && levelId is not null
                    && x.ReplayInfo?.hash != levelId) return;
                context.Send(y => AddReplayToList((IReplayHeader)y), x);
            });
            if (token.IsCancellationRequested) return;
            FinishReplayLoading(false);
        }

        private void FinishReplayLoading(bool isCancelled) {
            if (!isCancelled) _headers = _tempHeaders;
            else _replaysList.SetData(_headers, _previewBeatmapLevel is null);
            _tokenSource = null;
            ShowLoadingScreen(false);
            RefreshFilterInternal();
        }

        private void ShowLoadingScreen(bool show) {
            _loadingContainerObject.SetActive(show);
            _mainContainerCanvasGroup.alpha = show ? 0.2f : 1;
        }

        private void AddReplayToList(IReplayHeader header) {
            _replaysList.AddReplay(header, _previewBeatmapLevel is null, true);
        }

        #endregion

        #region Filters

        private Func<IReplayHeader, bool>? _predicate;
        private IPreviewBeatmapLevel? _previewBeatmapLevel;
        private IPreviewBeatmapLevel? _cachedPreviewBeatmapLevel;

        public void SetBeatmap(IPreviewBeatmapLevel? beatmapLevel, bool keepInCache = true, bool forceUpdate = false) {
            if (!_isInitialized) throw new UninitializedComponentException();
            if (_previewBeatmapLevel == beatmapLevel && !forceUpdate) return;
            CancelReplayInfosLoading();
            if (_cachedPreviewBeatmapLevel is not null
                && beatmapLevel == _cachedPreviewBeatmapLevel) {
                _previewBeatmapLevel = _cachedPreviewBeatmapLevel;
                _cachedPreviewBeatmapLevel = null;
                _replaysList.ShowEmptyScreen(false);
                return;
            }
            _cachedPreviewBeatmapLevel = null;
            if (beatmapLevel is null && keepInCache) {
                _cachedPreviewBeatmapLevel = _previewBeatmapLevel;
                _replaysList.ShowEmptyScreen(true);
                return;
            }
            _previewBeatmapLevel = beatmapLevel;
            _ = StartReplayInfosLoading(
                beatmapLevel?.levelID.Replace("custom_level_", ""),
                SynchronizationContext.Current);
        }

        public void Filter(Func<IReplayHeader, bool>? predicate) {
            _predicate = predicate;
            if (_tokenSource is not null) return;
            RefreshFilterInternal();
        }

        private void RefreshFilterInternal() {
            _replaysListSettingsPanel.ShowCorruptedInteractable =
                _previewBeatmapLevel is null && _predicate is null;
            if (_headers is null) return;
            var cells = _replaysList.Cells;
            cells.Clear();
            cells.AddRange(_replaysList.ActualCells
                .Where(cell => cell.ReplayHeader!.FileStatus is not
                    FileStatus.Corrupted && (_predicate?.Invoke(cell.ReplayHeader!) ?? true)));
            _replaysList.Refresh();
            RefreshSortingInternal();
        }

        #endregion

        #region Sorting

        private class AbstractDataCellComparator : IComparer<ReplaysList.AbstractDataCell> {
            public ReplaysListSettingsPanel.Sorters sorter;

            public int Compare(ReplaysList.AbstractDataCell x, ReplaysList.AbstractDataCell y) {
                var xi = x.ReplayHeader!.ReplayInfo;
                var yi = y.ReplayHeader!.ReplayInfo;
                return xi is null || yi is null ? 0 : sorter switch {
                    ReplaysListSettingsPanel.Sorters.Difficulty =>
                        CompareFloat(
                            (int)Enum.Parse(typeof(BeatmapDifficulty), xi.difficulty),
                            (int)Enum.Parse(typeof(BeatmapDifficulty), yi.difficulty)),
                    ReplaysListSettingsPanel.Sorters.Player =>
                        string.CompareOrdinal(xi.playerName, yi.playerName),
                    ReplaysListSettingsPanel.Sorters.Completion =>
                        CompareFloat((float)x.ReplayHeader.ReplayFinishType, (float)y.ReplayHeader.ReplayFinishType),
                    ReplaysListSettingsPanel.Sorters.Date =>
                        -CompareFloat(int.Parse(xi.timestamp), int.Parse(yi.timestamp)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            private static int CompareFloat(float x, float y) => x >= y ? Math.Abs(x - y) < 0.01 ? 0 : 1 : -1;
        }

        private AbstractDataCellComparator? _abstractDataCellComparator;
        private bool _ascendingOrder;

        private void Sort(ReplaysListSettingsPanel.Sorters sorter, bool ascending) {
            _abstractDataCellComparator ??= new();
            _abstractDataCellComparator.sorter = sorter;
            _ascendingOrder = ascending;
            if (_tokenSource is not null) return;
            RefreshSortingInternal();
        }

        private void RefreshSortingInternal() {
            if (_headers is null) return;
            _corruptedWasTurnedOn = false;
            ToggleCorruptedReplays(false);
            _replaysList.Cells.Sort(_abstractDataCellComparator);
            if (!_ascendingOrder) _replaysList.Cells.Reverse();
            _replaysList.Refresh();
            _replayPanel.SetData(null);
        }

        #endregion

        #region ToggleCorruptedReplays

        private readonly List<ReplaysList.AbstractDataCell> _originalCells = new();
        private bool _corruptedWasTurnedOn;
        private bool _showCorrupted;

        private void ToggleCorruptedReplays(bool show) {
            if (show == _showCorrupted) return;
            _showCorrupted = show;
            if (_tokenSource is not null) return;
            RefreshCorruptedReplaysInternal();
        }

        private void RefreshCorruptedReplaysInternal() {
            if (_headers is null || _previewBeatmapLevel is not null) return;
            var cells = _replaysList.Cells;
            if (_showCorrupted) {
                _corruptedWasTurnedOn = true;
                _originalCells.Clear();
                _originalCells.AddRange(cells);
                cells.Clear();
                cells.AddRange(_replaysList.ActualCells
                    .Where(x => x.ReplayHeader!.FileStatus is FileStatus.Corrupted));
            } else if (_corruptedWasTurnedOn) {
                cells.Clear();
                cells.AddRange(_originalCells);
                _originalCells.Clear();
            }
            _replaysList.Refresh();
        }

        #endregion

        #region Callbacks

        private IReplayHeader? _selectedHeader;

        private void HandleReplayAdded(IReplayHeader header) {
            _replaysList.AddReplay(header);
            RefreshSortingInternal();
            _replaysListSettingsPanel.ShowCorrupted = false;
        }

        private void HandleReplayDeleted(IReplayHeader header) {
            if (_selectedHeader == header) _replayPanel.SetData(null);
            _replaysList.RemoveReplay(header, true);
        }

        private void HandleReplaysDeleted(string[]? removedPaths) {
            if (removedPaths is null) return;
            //convert it to dict to not enumerate the whole array since dicts use hash maps
            var dict = removedPaths.ToDictionary(x => x);
            CancelReplayInfosLoading();
            _replaysList.SetData(_replaysList.ActualCells
                .Select(static x => x.ReplayHeader)
                .Where(x => x is not null && !dict.ContainsKey(x.FilePath))!);
            if (_selectedHeader is null || dict.ContainsKey(_selectedHeader.FilePath)) {
                _replayPanel.SetData(null);
            }
        }

        private void HandleShowEmptyScreenChanged(bool show) {
            //if (!_isInitialized) return;
            //_settingsPanelContainerObject.SetActive(!show);
        }

        private void HandleShowCorruptedChangedEvent(bool show) {
            ToggleCorruptedReplays(show);
        }

        private void HandleSorterChanged(ReplaysListSettingsPanel.Sorters sorters, bool ascending) {
            ToggleCorruptedReplays(false);
            Sort(sorters, ascending);
        }

        private void HandleReplaySelected(ReplaysList.AbstractDataCell? cell) {
            if (cell is null) return;
            _selectedHeader = cell.ReplayHeader;
            _replayPanel.SetData(_selectedHeader);
        }

        [UIAction("cancel-loading"), UsedImplicitly]
        private void HandleCancelLoadingButtonClicked() {
            CancelReplayInfosLoading();
        }

        #endregion
    }
}