using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal interface IReplayFilter : IBeatmapFilter {
        string? PlayerName { get; }
    }

    internal interface IBeatmapReplayLaunchPanel {
        IListComponent<IReplayHeader> List { get; }
        IReplayFilter? Filter { get; set; }
    }

    internal class BeatmapReplayLaunchPanel : ReeUIComponentV3<BeatmapReplayLaunchPanel>, IBeatmapReplayLaunchPanel {
        #region DetailPanel

        public interface IDetailPanel {
            bool AllowReplayMultiselect { get; }

            void SetData(IBeatmapReplayLaunchPanel launchPanel, IReadOnlyList<IReplayHeader> headers);

            void OnStateChange(bool active);
        }

        #endregion

        #region UI Components

        IListComponent<IReplayHeader> IBeatmapReplayLaunchPanel.List => _replaysList;

        [UIComponent("replays-list"), UsedImplicitly]
        private ReplaysList _replaysList = null!;

        [UIComponent("replays-list-settings-panel"), UsedImplicitly]
        private ReplaysListSettingsPanel _replaysListSettingsPanel = null!;

        [UIComponent("replay-detail"), UsedImplicitly]
        private ReplayDetailPanel _replayDetail = null!;

        [UIComponent("battle-royale-detail"), UsedImplicitly]
        private BattleRoyaleDetailPanel _battleRoyaleDetail = null!;

        [UIComponent("details-container"), UsedImplicitly]
        private ObjectSwitcher _detailsContainer = null!;

        [UIComponent("replays-list-scrollbar"), UsedImplicitly]
        private Scrollbar _replaysListScrollbar = null!;

        [UIObject("loading-container"), UsedImplicitly]
        private GameObject _loadingContainerObject = null!;

        [UIObject("main-container"), UsedImplicitly]
        private GameObject _mainContainerObject = null!;

        private CanvasGroup _mainContainerCanvasGroup = null!;

        #endregion

        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<ReplayMode>? ReplayModeChangedEvent;

        #endregion

        #region Init

        private IReplayManager _replayManager = null!;
        private bool _isInitialized;

        public void Setup(IReplayManager replayManager, ReplayerMenuLoader loader) {
            _replayManager = replayManager;
            _replayManager.ReplaysDeletedEvent += HandleReplaysDeleted;
            _replayManager.ReplayDeletedEvent += HandleReplayDeleted;
            _replayManager.ReplayAddedEvent += HandleReplayAdded;
            _replayDetail.SetData(null);
            _isInitialized = true;
        }

        protected override void OnInitialize() {
            var image = _loadingContainerObject.AddComponent<Image>();
            image.sprite = BundleLoader.TransparentPixel;
            image.color = Color.clear;
            _mainContainerCanvasGroup = _mainContainerObject.AddComponent<CanvasGroup>();
            _replaysList.Scrollbar = _replaysListScrollbar;
            _replaysList.ItemsWithIndexesSelectedEvent += HandleItemsSelected;
            _detailPanel = _replayDetail;
            CurrentReplayMode = ReplayMode.Standard;
        }

        protected override void OnDispose() {
            _replayManager.ReplaysDeletedEvent -= HandleReplaysDeleted;
            _replayManager.ReplayDeletedEvent -= HandleReplayDeleted;
            _replayManager.ReplayAddedEvent -= HandleReplayAdded;
        }

        #endregion

        #region Data Management

        private List<IReplayHeader> ListHeaders => _replaysList.items;

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
            ShowLoadingScreen(true);
            _tokenSource = new();
            ListHeaders.Clear();
            var token = _tokenSource.Token;
            var context = SynchronizationContext.Current;
            _tempHeaders = await _replayManager.LoadReplayHeadersAsync(token, x => {
                if (x.FileStatus is FileStatus.Corrupted || ListHeaders.Count > 10) return;
                context.Send(y => AddReplayToList((IReplayHeader)y), x);
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
            else {
                ListHeaders.Clear();
                if (_headers is not null) ListHeaders.AddRange(_headers);
                _replaysList.Refresh();
            }
            _cachedShowBeatmapNameIfCorrect = _showBeatmapNameIfCorrect;
            _tokenSource = null;
            ShowLoadingScreen(false);
            RefreshFilter();
        }

        private void RemoveReplayFromList(IReplayHeader header, bool refresh = true) {
            ListHeaders.Remove(header);
            if (refresh) _replaysList.Refresh();
        }

        private void AddReplayToList(IReplayHeader header) {
            ListHeaders.Add(header);
            _replaysList.Refresh();
        }

        private void ShowLoadingScreen(bool show) {
            _loadingContainerObject.SetActive(show);
            _mainContainerCanvasGroup.alpha = show ? 0.2f : 1;
        }

        #endregion

        #region Filtering

        public IReplayFilter? Filter {
            get => _filter;
            set {
                if (_filter is not null) _filter.FilterUpdatedEvent -= RefreshFilter;
                _filter = value;
                if (_filter is not null) _filter.FilterUpdatedEvent += RefreshFilter;
                if (_tokenSource is not null) return;
                RefreshFilter();
            }
        }

        private IReplayFilter? _filter;

        public void RefreshFilter() {
            if (_headers is null) return;
            ListHeaders.Clear();
            if (_filter is not null) {
                var filtered = _headers.Where(cell =>
                    cell.FileStatus is not FileStatus.Corrupted && FilterPredicate(cell));
                ListHeaders.AddRange(filtered);
            }
            _replaysList.Refresh();
            _replayDetail.SetData(null);
        }

        private bool FilterPredicate(IReplayHeader header) {
            var player = _filter!.PlayerName;
            var level = _filter.BeatmapLevel?.levelID;
            var characteristic = _filter.BeatmapCharacteristic?.serializedName;
            var diff = _filter.BeatmapDifficulty;
            return header.ReplayInfo is not { } info ||
                (player is null || info.PlayerName.ToLower().Contains(player))
                && (!_filter.Enabled || level?.Replace("custom_level_", "") == info.SongHash)
                && (!diff.HasValue || info.SongDifficulty == diff.Value.ToString())
                && (characteristic is null || info.SongMode == characteristic);
        }

        #endregion

        #region Sorting

        private void RefreshSortingInternal() {
            if (_headers is null) return;
            _replaysList.Refresh();
            _replayDetail.SetData(null);
        }

        #endregion

        #region Replay Mode

        public enum ReplayMode {
            Standard,
            BattleRoyale
        }

        private ReplayMode CurrentReplayMode {
            get => _currentReplayMode;
            set {
                _currentReplayMode = value;
                _detailsContainer.ShowObjectWithIndex(value is ReplayMode.BattleRoyale ? 1 : 0);
                _detailPanel.OnStateChange(false);
                _detailPanel = value is ReplayMode.BattleRoyale ? _battleRoyaleDetail : _replayDetail;
                _detailPanel.SetData(this, Array.Empty<IReplayHeader>());
                _detailPanel.OnStateChange(true);
                _replaysList.AllowMultiselect = _detailPanel.AllowReplayMultiselect;
                ReplayModeChangedEvent?.Invoke(value);
            }
        }

        private ReplayMode _currentReplayMode;
        private IDetailPanel _detailPanel = null!;

        #endregion
        
        #region Callbacks

        private void HandleReplayAdded(IReplayHeader header) {
            AddReplayToList(header);
            RefreshSortingInternal();
        }

        private void HandleReplayDeleted(IReplayHeader header) {
            _replayDetail.SetData(null);
            RemoveReplayFromList(header);
        }

        private void HandleReplaysDeleted(string[]? removedPaths) {
            if (removedPaths is null) return;
            CancelReplayInfosLoading();
            //convert it to hash set to avoid array enumeration
            var dict = new HashSet<string>(removedPaths);
            var replaysToKeep = ListHeaders
                .Where(x => !dict.Contains(x.FilePath));
            ListHeaders.Clear();
            ListHeaders.AddRange(replaysToKeep);
            _replaysList.Refresh();
            _replayDetail.SetData(null);
        }

        private void HandleItemsSelected(ICollection<int> items) {
            _detailPanel.SetData(this, ListHeaders.TakeIndexes(items));
        }

        [UIAction("replay-mode-change"), UsedImplicitly]
        private void HandleReplayModeChanged(ReplayMode mode) {
            CurrentReplayMode = mode;
        }

        [UIAction("sorter-change"), UsedImplicitly]
        private void HandleSorterChanged(ReplaysList.Sorter sorter, SortOrder sortOrder) {
            _replaysList.SortBy = sorter;
            _replaysList.SortOrder = sortOrder;
            RefreshSortingInternal();
        }

        [UIAction("reload-data"), UsedImplicitly]
        private void HandleReloadButtonClicked() {
            ReloadData(_cachedShowBeatmapNameIfCorrect);
        }

        [UIAction("cancel-loading"), UsedImplicitly]
        private void HandleCancelLoadingButtonClicked() {
            CancelReplayInfosLoading();
        }

        #endregion
    }
}