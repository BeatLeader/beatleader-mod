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
            _replaysList.SetData(_listHeaders);
        }

        protected override void OnInstantiate() {
            _replaysList = Instantiate<ReplaysList>(transform);
            _replaysListSettingsPanel = Instantiate<ReplaysListSettingsPanel>(transform);
            _replayPanel = Instantiate<ReplayDetailPanel>(transform);
            _replaysList.ReplaySelectedEvent += HandleReplaySelected;
            _replaysListSettingsPanel.SorterChangedEvent += HandleSorterChanged;
            _replaysListSettingsPanel.ReloadDataEvent += ReloadDataInternal;
        }

        protected override void OnDispose() {
            _replayManager.ReplaysDeletedEvent -= HandleReplaysDeleted;
            _replayManager.ReplayDeletedEvent -= HandleReplayDeleted;
            _replayManager.ReplayAddedEvent -= HandleReplayAdded;
            _replaysListSettingsPanel.ReloadDataEvent -= ReloadDataInternal;
        }

        private void ReloadDataInternal() {
            ReloadData(_cachedShowBeatmapNameIfCorrect);
        }

        #endregion

        #region Data Management

        private readonly List<IReplayHeader> _listHeaders = new();

        private IList<IReplayHeader>? _headers;
        private IList<IReplayHeader>? _tempHeaders;
        private CancellationTokenSource? _tokenSource;

        private bool _cachedShowBeatmapNameIfCorrect;
        private bool _showBeatmapNameIfCorrect;

        public void ReloadData(bool showBeatmapNameIfCorrect = true) {
            if (!_isInitialized) throw new UninitializedComponentException();
            _ = StartReplayInfosLoading(showBeatmapNameIfCorrect);
        }

        private async Task StartReplayInfosLoading(bool showBeatmapNameIfCorrect) {
            if (_tokenSource != null) return;
            _showBeatmapNameIfCorrect = showBeatmapNameIfCorrect;
            _replaysList.SetData();
            ShowLoadingScreen(true);
            _tokenSource = new();
            var token = _tokenSource.Token;
            var context = SynchronizationContext.Current;
            _tempHeaders = await _replayManager.LoadReplayHeadersAsync(token, x => {
                if (x.FileStatus is FileStatus.Corrupted || _listHeaders.Count > ReplaysList.VisibleCells) return;
                context.Send(x => AddReplayToList((IReplayHeader)x), x);
            });
            if (token.IsCancellationRequested) return;
            FinishReplayLoading(false);
        }

        private void CancelReplayInfosLoading() {
            if (_tokenSource == null) return;
            _tokenSource.Cancel();
            FinishReplayLoading(true);
        }

        private void FinishReplayLoading(bool isCancelled) {
            if (!isCancelled) _headers = _tempHeaders;
            else  {
                _listHeaders.Clear();
                if (_headers is not null) _listHeaders.AddRange(_headers);
                _replaysList.Refresh();
            }
            _cachedShowBeatmapNameIfCorrect = _showBeatmapNameIfCorrect;
            _tokenSource = null;
            ShowLoadingScreen(false);
            RefreshFilterInternal();
        }

        private void RemoveReplayFromList(IReplayHeader header) {
            _listHeaders.Remove(header);
            _replaysList.Refresh();
        }

        private void AddReplayToList(IReplayHeader header) {
            _listHeaders.Add(header);
            _replaysList.Refresh();
        }

        #endregion

        #region ShowLoadingScreen

        private void ShowLoadingScreen(bool show) {
            _loadingContainerObject.SetActive(show);
            _mainContainerCanvasGroup.alpha = show ? 0.2f : 1;
        }

        #endregion

        #region Filter

        private Func<IReplayHeader, bool>? _predicate;

        public void SetFilter(Func<IReplayHeader, bool>? predicate) {
            _predicate = predicate;
            if (_tokenSource is not null) return;
            RefreshFilterInternal();
        }

        private void RefreshFilterInternal() {
            if (_headers is null) return;
            _listHeaders.Clear();
            _listHeaders.AddRange(_headers.Where(cell =>
                cell.FileStatus is not FileStatus.Corrupted
                && (_predicate?.Invoke(cell) ?? true)));
            _replaysList.Refresh();
            RefreshSortingInternal();
        }

        #endregion

        #region Sorting

        private class HeaderComparator : IComparer<IReplayHeader> {
            public ReplaysListSettingsPanel.Sorters sorter;

            public int Compare(IReplayHeader x, IReplayHeader y) {
                var xi = x.ReplayInfo;
                var yi = y.ReplayInfo;
                return xi is null || yi is null ? 0 : sorter switch {
                    ReplaysListSettingsPanel.Sorters.Difficulty =>
                        CompareFloat(
                            (int)Enum.Parse(typeof(BeatmapDifficulty), xi.SongDifficulty),
                            (int)Enum.Parse(typeof(BeatmapDifficulty), yi.SongDifficulty)),
                    ReplaysListSettingsPanel.Sorters.Player =>
                        string.CompareOrdinal(xi.PlayerName, yi.PlayerName),
                    ReplaysListSettingsPanel.Sorters.Completion =>
                        CompareFloat((float)xi.LevelEndType, (float)yi.LevelEndType),
                    ReplaysListSettingsPanel.Sorters.Date =>
                        -CompareFloat(int.Parse(xi.Timestamp), int.Parse(yi.Timestamp)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            private static int CompareFloat(float x, float y) => x >= y ? Math.Abs(x - y) < 0.01 ? 0 : 1 : -1;
        }

        private HeaderComparator? _headerComparator;
        private bool _ascendingOrder;

        public void SetSorter(ReplaysListSettingsPanel.Sorters sorter, bool ascending) {
            _headerComparator ??= new();
            _headerComparator.sorter = sorter;
            _ascendingOrder = ascending;
            if (_tokenSource is not null) return;
            RefreshSortingInternal();
        }

        private void RefreshSortingInternal() {
            if (_headers is null) return;
            _listHeaders.Sort(_headerComparator);
            if (!_ascendingOrder) _listHeaders.Reverse();
            _replaysList.Refresh();
            _replayPanel.SetData(null);
        }

        #endregion

        #region Callbacks

        private IReplayHeader? _selectedHeader;

        private void HandleReplayAdded(IReplayHeader header) {
            AddReplayToList(header);
            RefreshSortingInternal();
            _replaysListSettingsPanel.ShowCorrupted = false;
        }

        private void HandleReplayDeleted(IReplayHeader header) {
            if (_selectedHeader == header) _replayPanel.SetData(null);
            RemoveReplayFromList(header);
        }

        private void HandleReplaysDeleted(string[]? removedPaths) {
            if (removedPaths is null) return;
            //convert it to hash set to not enumerate the array
            var dict = new HashSet<string>(removedPaths);
            CancelReplayInfosLoading();
            _replaysList.SetData(_listHeaders
                .Where(x => !dict.Contains(x.FilePath))
                .ToArray());
            _replayPanel.SetData(null);
        }

        private void HandleSorterChanged(ReplaysListSettingsPanel.Sorters sorters, bool ascending) {
            SetSorter(sorters, ascending);
        }

        private void HandleReplaySelected(IReplayHeader? header) {
            if (header is null) return;
            _selectedHeader = header;
            _replayPanel.SetData(_selectedHeader);
        }

        [UIAction("cancel-loading"), UsedImplicitly]
        private void HandleCancelLoadingButtonClicked() {
            CancelReplayInfosLoading();
        }

        #endregion
    }
}