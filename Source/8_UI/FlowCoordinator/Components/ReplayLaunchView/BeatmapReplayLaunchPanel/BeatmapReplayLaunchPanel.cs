using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class BeatmapReplayLaunchPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("replays-list"), UsedImplicitly]
        private ReplaysList _replaysList = null!;

        [UIComponent("replays-list-settings-panel"), UsedImplicitly]
        private ReplaysListSettingsPanel _replaysListSettingsPanel = null!;

        [UIValue("replay-panel"), UsedImplicitly]
        private ReplayDetailPanel _replayPanel = null!;

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
            var image = _loadingContainerObject.AddComponent<Image>();
            image.sprite = BundleLoader.TransparentPixel;
            image.color = Color.clear;
            _mainContainerCanvasGroup = _mainContainerObject.AddComponent<CanvasGroup>();
            _replaysList.AllowMultiselect = true;
        }

        protected override void OnInstantiate() {
            _replayPanel = Instantiate<ReplayDetailPanel>(transform);
        }

        protected override void OnDispose() {
            _replayManager.ReplaysDeletedEvent -= HandleReplaysDeleted;
            _replayManager.ReplayDeletedEvent -= HandleReplayDeleted;
            _replayManager.ReplayAddedEvent -= HandleReplayAdded;
        }

        [UsedImplicitly]
        private void ReloadDataInternal() {
            ReloadData(_cachedShowBeatmapNameIfCorrect);
        }

        #endregion

        #region Data Management

        private List<IReplayHeader> ListHeaders => _replaysList.replays;

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
                if (x.FileStatus is FileStatus.Corrupted || ListHeaders.Count > ReplaysList.VisibleCells) return;
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
            RefreshFilterInternal();
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

        #region Filter

        private Func<IReplayHeader, bool>? _predicate;

        public void FilterBy(Func<IReplayHeader, bool>? predicate) {
            _predicate = predicate;
            if (_tokenSource is not null) return;
            RefreshFilterInternal();
        }

        private void RefreshFilterInternal() {
            if (_headers is null) return;
            ListHeaders.Clear();
            ListHeaders.AddRange(_headers.Where(cell =>
                cell.FileStatus is not FileStatus.Corrupted
                && (_predicate?.Invoke(cell) ?? true)));
            _replaysList.Refresh();
            _replayPanel.SetData(null);
        }

        #endregion

        #region Sorting

        private void RefreshSortingInternal() {
            if (_headers is null) return;
            _replaysList.Refresh();
            _replayPanel.SetData(null);
        }

        #endregion

        #region Battle Royale

        private bool BattleRoyaleEnabled {
            get;
            set;
        }

        #endregion

        #region Callbacks

        private IReplayHeader? _selectedHeader;

        private void HandleReplayAdded(IReplayHeader header) {
            AddReplayToList(header);
            RefreshSortingInternal();
        }

        private void HandleReplayDeleted(IReplayHeader header) {
            if (_selectedHeader == header) _replayPanel.SetData(null);
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
            _replayPanel.SetData(null);
        }

        [UsedImplicitly]
        private void HandleSorterChanged(ReplaysList.Sorter sorter, SortOrder sortOrder) {
            _replaysList.SortBy = sorter;
            _replaysList.SortOrder = sortOrder;
        }

        [UsedImplicitly]
        private void HandleReplaysSelected(IReplayHeader[]? headers) {
            if (headers is null) return;
            // _selectedHeader = headers;
            // _replayPanel.SetData(_selectedHeader);
        }

        [UIAction("cancel-loading"), UsedImplicitly]
        private void HandleCancelLoadingButtonClicked() {
            CancelReplayInfosLoading();
        }

        #endregion
    }
}