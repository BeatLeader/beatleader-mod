using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using static BeatLeader.Models.FileStatus;
using ModalSystemHelper = BeatLeader.UI.Reactive.Components.ModalSystemHelper;

namespace BeatLeader.UI.Hub {
    //TODO: rewrite beatmap downloading (BEFORE MERGING!)
    internal class ReplayDetailPanel : ReeUIComponentV3<ReplayDetailPanel>, BeatmapReplayLaunchPanel.IDetailPanel {
        #region Configuration

        private const string WatchText = "Watch";
        private const string DownloadText = "Download map";

        #endregion

        #region UI Components

        [UIValue("replay-info-panel"), UsedImplicitly]
        private ReplayStatisticsPanel _replayStatisticsPanel = null!;

        [UIValue("download-beatmap-panel"), UsedImplicitly]
        private DownloadBeatmapPanel _downloadBeatmapPanel = null!;

        [UIComponent("mini-profile"), UsedImplicitly]
        private QuickMiniProfile _miniProfile = null!;

        #endregion

        #region Action Buttons

        [UIValue("watch-button-text"), UsedImplicitly]
        private string? WatchButtonText {
            get => _watchButtonText;
            set {
                _watchButtonText = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("watch-button-interactable"), UsedImplicitly]
        private bool WatchButtonInteractable {
            get => _watchButtonInteractable;
            set {
                _watchButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("delete-button-interactable"), UsedImplicitly]
        private bool DeleteButtonInteractable {
            get => _deleteButtonInteractable;
            set {
                _deleteButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        private bool _deleteButtonInteractable;
        private bool _watchButtonInteractable;
        private string? _watchButtonText;

        #endregion

        #region Setup

        private ReplayerMenuLoader? _replayerMenuLoader;

        public void Setup(ReplayerMenuLoader menuLoader) {
            _replayerMenuLoader = menuLoader;
        }

        void BeatmapReplayLaunchPanel.IDetailPanel.Setup(IBeatmapReplayLaunchPanel? launchPanel, Transform? parent) {
            _beatmapReplayLaunchPanel = launchPanel;
            ContentTransform.SetParent(parent, false);
            Content.SetActive(parent is not null);
        }

        protected override void OnInstantiate() {
            _replayStatisticsPanel = ReeUIComponentV2.Instantiate<ReplayStatisticsPanel>(transform);
            _downloadBeatmapPanel = ReeUIComponentV2.Instantiate<DownloadBeatmapPanel>(transform);

            _replayStatisticsPanel.SetData(null, null, true, true);

            _downloadBeatmapPanel.BackButtonClickedEvent += HandleDownloadMenuBackButtonClicked;
            _downloadBeatmapPanel.DownloadAbilityChangedEvent += HandleDownloadAbilityChangedEvent;

            _tagSelector = new();
            _tagSelector.Component.SelectedTagAddedEvent += x => _header?.ReplayMetadata.Tags.Add(x);
            _tagSelector.Component.SelectedTagRemovedEvent += x => _header?.ReplayMetadata.Tags.Remove(x);
            _tagSelector.Component.Setup(ReplayMetadataManager.Instance);

            SetDownloadPanelActive(false);
        }

        protected override void OnInitialize() {
            _miniProfile.SetPlayer(null);
        }

        protected override bool OnValidation() {
            return _replayerMenuLoader is not null;
        }

        #endregion

        #region Download Panel

        private void SetDownloadPanelActive(bool active) {
            _downloadBeatmapPanel.SetRootActive(active);
            _replayStatisticsPanel.SetRootActive(!active);
            _isIntoDownloadMenu = active;
        }

        #endregion

        #region Data

        private CancellationTokenSource _cancellationTokenSource = new();
        private IBeatmapReplayLaunchPanel? _beatmapReplayLaunchPanel;
        private IReplayHeader? _header;

        private bool _beatmapIsMissing;
        private bool _isIntoDownloadMenu;
        private bool _isWorking;

        public void SetData(IReplayHeader? header) {
            if (_isWorking) {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new();
            }
            if (_isIntoDownloadMenu) SetDownloadPanelActive(false);
            _isWorking = true;
            var invalid = header is null || header.FileStatus is Corrupted;
            _replayStatisticsPanel.SetData(null, null, invalid, header is null);
            _header = header;
            if (_header != null) {
                _tagSelector.Component.SelectFromMetadata(_header.ReplayMetadata);
            } else {
                _tagSelector.Component.ClearSelectedTags();
            }
            DeleteButtonInteractable = header is not null;
            WatchButtonInteractable = false;
            if (!invalid) {
                _ = ProcessDataAsync(header!, _cancellationTokenSource.Token);
                return;
            }
            _miniProfile.SetPlayer(null);
            _isWorking = false;
        }

        private async Task ProcessDataAsync(IReplayHeader header, CancellationToken token) {
            var playerTask = header.LoadPlayerAsync(false, token);
            DeleteButtonInteractable = false;
            var replay = await header.LoadReplayAsync(token);
            if (token.IsCancellationRequested) return;
            DeleteButtonInteractable = true;
            var stats = default(ScoreStats?);
            var score = default(Score?);
            if (replay is not null) {
                await Task.Run(() => stats = ReplayStatisticUtils.ComputeScoreStats(replay), token);
                score = ReplayUtils.ComputeScore(replay);
                score.fcAccuracy = stats?.accuracyTracker.fcAcc ?? 0;
            }
            if (token.IsCancellationRequested) return;
            _replayStatisticsPanel.SetData(score, stats, score is null || stats is null);
            await playerTask;
            _miniProfile.SetPlayer(playerTask.Result);
            await RefreshAvailabilityAsync(header.ReplayInfo!, token);
        }

        private async Task RefreshAvailabilityAsync(IReplayInfo info, CancellationToken token) {
            var beatmap = await _replayerMenuLoader!.LoadBeatmapAsync(
                info.SongHash,
                info.SongMode,
                info.SongDifficulty,
                token
            );
            if (token.IsCancellationRequested) return;
            var invalid = beatmap is null;
            WatchButtonText = invalid ? DownloadText : WatchText;
            WatchButtonInteractable = invalid || SongCoreInterop.ValidateRequirements(beatmap!);
            _beatmapIsMissing = invalid;
            if (invalid) _downloadBeatmapPanel.SetHash(info.SongHash);
            _isWorking = false;
        }

        #endregion

        #region Callbacks

        [UIComponent("tags-button")]
        private RectTransform _tagsButton = null!;

        private Modal<TagSelector> _tagSelector = null!;

        private void HandleDownloadAbilityChangedEvent(bool ableToDownload) {
            WatchButtonInteractable = ableToDownload;
        }

        private void HandleDownloadMenuBackButtonClicked() {
            SetDownloadPanelActive(false);
            _isIntoDownloadMenu = false;
            _ = RefreshAvailabilityAsync(_header!.ReplayInfo!, _cancellationTokenSource.Token);
        }

        [UIAction("tags-button-click"), UsedImplicitly]
        private void HandleTagsButtonClicked() {
            //TODO: temporary solution until replay detail migration
            ModalSystemHelper.OpenModalRelatively(
                _tagSelector,
                ContentTransform,
                _tagsButton,
                ModalSystemHelper.RelativePlacement.BottomRight,
                shadowSettings: new()
            );
        }

        [UIAction("delete-button-click"), UsedImplicitly]
        private void HandleDeleteButtonClicked() {
            _header?.DeleteReplay();
        }

        [UIAction("watch-button-click"), UsedImplicitly]
        private async void HandleWatchButtonClicked() {
            if (_isIntoDownloadMenu) {
                _downloadBeatmapPanel.NotifyDownloadButtonClicked();
                return;
            }
            if (_beatmapIsMissing) {
                SetDownloadPanelActive(true);
                return;
            }
            if (_header is null || _header.FileStatus is Corrupted) return;
            ValidateAndThrow();
            var replay = await _header.LoadReplayAsync(default);
            var player = await _header.LoadPlayerAsync(false, default);
            await _replayerMenuLoader!.StartReplayAsync(replay!, player);
        }

        #endregion
    }
}