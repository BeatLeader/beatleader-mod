using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using BeatLeader.Models.Replay;
using BeatLeader.UI;
using BeatLeader.Utils;
using Reactive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = Reactive.BeatSaber.Components.Image;
using BeatLeader.API;
using Reactive.BeatSaber.Components;

namespace BeatLeader.Components {
    internal class ReplayPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("download-text"), UsedImplicitly]
        private TMP_Text _downloadText = null!;

        [UIComponent("play-button"), UsedImplicitly]
        private Button _playButton = null!;

        [UIComponent("play-button"), UsedImplicitly]
        private TMP_Text _playButtonText = null!;

        [UIComponent("download-button"), UsedImplicitly]
        private Button _downloadButton = null!;

        [UIComponent("download-button"), UsedImplicitly]
        private TMP_Text _downloadButtonText = null!;

        private Image _downloadButtonImage = null!;
        private Spinner _downloadButtonSpinner = null!;

        [UIValue("settings-panel"), UsedImplicitly]
        private ReplayerSettingsPanel _settingsPanel = null!;

        #endregion

        #region Events

        public event Action<bool>? DownloadStateChangedEvent;

        private void NotifyDownloadStateChanged(bool state) {
            DownloadStateChangedEvent?.Invoke(state);
        }

        #endregion

        #region Initialize/Dispose

        private ReplayerViewNavigatorWrapper? _replayerNavigator;
        private bool _isWaitingForReplayInfos;

        public void Setup(ReplayerViewNavigatorWrapper starter) {
            _replayerNavigator = starter;
        }

        protected override void OnInstantiate() {
            _settingsPanel = Instantiate<ReplayerSettingsPanel>(transform);
        }

        protected override void OnInitialize() {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _downloadButton.onClick.AddListener(OnDownloadButtonClicked);

            _downloadButtonImage = new Image {
                Sprite = BundleLoader.SaveIcon,
                Color = Color.white * 0.8f,
                PreserveAspect = true,
                Skew = UIStyle.Skew
            }.With(x => {
                    x.WithNativeComponent(out LayoutElement el);
                    el.preferredHeight = 6f;
                    el.preferredWidth = 6f;
                }
            );

            _downloadButtonImage.Use(_downloadButtonText.transform.parent);

            _downloadButtonSpinner = new Spinner().With(x => {
                    x.WithNativeComponent(out LayoutElement el);
                    el.preferredHeight = 4.5f;
                    el.preferredWidth = 4.5f;
                }
            );
            _downloadButtonSpinner.Use(_downloadButtonText.transform.parent);
            _downloadButtonSpinner.Enabled = false;

            StaticReplayRequest.ProgressChangedEvent += OnDownloadProgressChanged;
            StaticReplayRequest.StateChangedEvent += OnDownloadRequestStateChanged;

            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
            ReplayManager.LoadingStartedEvent += OnReplayInfosLoadingStarted;
            ReplayManager.LoadingFinishedEvent += OnReplayInfosLoadingFinished;
            ReplayManager.ReplayAddedEvent += OnReplayAdded;
        }

        protected override void OnDispose() {
            StaticReplayRequest.ProgressChangedEvent -= OnDownloadProgressChanged;
            StaticReplayRequest.StateChangedEvent -= OnDownloadRequestStateChanged;

            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapChanged);
            ReplayManager.LoadingStartedEvent -= OnReplayInfosLoadingStarted;
            ReplayManager.LoadingFinishedEvent -= OnReplayInfosLoadingFinished;
            ReplayManager.ReplayAddedEvent -= OnReplayAdded;
        }

        protected override void OnRootStateChange(bool active) {
            if (!active) {
                return;
            }

            ReplayManager.StartLoadingIfNeverLoaded();
            SyncReplayInfoLoadingState();

            if (_score != null) {
                SetScore(_score);
            } else {
                RefreshDownloadButton(_isWaitingForReplayInfos ? DownloadButtonState.LoadingReplayInfos : DownloadButtonState.ReadyToDownload);
                RefreshPlayButton(PlayButtonState.ReadyToDownloadOrStart);
            }
        }

        #endregion

        #region SetScore

        private Score? _score;
        private IReplayHeader? _replayHeader;

        public void SetScore(Score score) {
            _score = score;
            _replayHeader = ReplayManager.FindReplayByHash(_score);
            SyncReplayInfoLoadingState();
            ResetButtons();
        }

        #endregion

        #region StartReplay

        private async Task StartReplay(Replay replay) {
            await _replayerNavigator!.NavigateToReplayAsync(replay, _score!.Player, true).RunCatching();

            SendViewReplayRequest.Send(_score.id);
        }

        private async Task LoadAndStartReplay() {
            if (_replayHeader == null) {
                throw new InvalidOperationException("Replay header must not be null");
            }

            var replay = await _replayHeader.LoadReplayAsync(CancellationToken.None);
            await StartReplay(replay!);
        }

        #endregion

        #region Callbacks

        private bool _blockIncomingEvents = true;
        private bool _isWaitingToStart;
        private bool _isDownloading;

        private void OnSelectedBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
            _playCanBeInteractable = SongCoreInterop.ValidateRequirements(new(level, key));
        }

        private void OnDownloadProgressChanged(WebRequests.IWebRequest<Replay> instance, float downloadProgress, float uploadProgress, float overallProgress) {
            if (_blockIncomingEvents) {
                return;
            }
            _downloadText.text = $"<alpha=#66>Downloading: {downloadProgress * 100:F0}%";
        }

        private void OnDownloadRequestStateChanged(WebRequests.IWebRequest<Replay> instance, WebRequests.RequestState state, string? failReason) {
            if (_blockIncomingEvents) {
                return;
            }

            _isDownloading = state is WebRequests.RequestState.Started;
            NotifyDownloadStateChanged(_isDownloading);

            switch (state) {
                case WebRequests.RequestState.Started:
                    _downloadText.text = "<alpha=#66>Starting...";

                    RefreshPlayButton(_isWaitingToStart ? PlayButtonState.Downloading : PlayButtonState.Unavailable);
                    RefreshDownloadButton(_isWaitingToStart ? DownloadButtonState.Unavailable : DownloadButtonState.Downloading);

                    return;
                case WebRequests.RequestState.Finished:
                    _downloadText.text = "<alpha=#66>Finished!";

                    // When initiated using the play button
                    if (_isWaitingToStart) {
                        RefreshDownloadButton(DownloadButtonState.Unavailable);
                        RefreshPlayButton(PlayButtonState.Unavailable);

                        StartReplay(instance.Result).RunCatching();
                    }
                    // When initiated using the download button
                    else {
                        Task.Run(async () => {
                                var result = await ReplayManager.SaveAnyReplayAsync(instance.Result!, null, CancellationToken.None);
                                _replayHeader = result.Header;
                            }
                        ).RunCatching();

                        RefreshDownloadButton(DownloadButtonState.ReadyToNavigate);
                        RefreshPlayButton(PlayButtonState.ReadyToDownloadOrStart);
                    }

                    return;
                case WebRequests.RequestState.Failed:
                    ResetButtons();

                    _downloadText.text = FormatFailString(failReason);
                    return;
            }
        }

        private void OnReplayInfosLoadingStarted() {
            SyncReplayInfoLoadingState();

            if (_score == null || _isDownloading) {
                return;
            }

            ResetButtons();
        }

        private void OnReplayInfosLoadingFinished(bool _) {
            if (_score != null) {
                _replayHeader = ReplayManager.FindReplayByHash(_score);
            }

            SyncReplayInfoLoadingState();

            if (_score == null || _isDownloading) {
                return;
            }

            ResetButtons();
        }

        private void OnReplayAdded(IReplayHeader _) {
            if (_score == null || _replayHeader != null || _isDownloading) {
                return;
            }

            var replayHeader = ReplayManager.FindReplayByHash(_score);
            if (replayHeader == null) {
                return;
            }

            _replayHeader = replayHeader;
            SyncReplayInfoLoadingState();
            ResetButtons();
        }

        #endregion

        #region Button Callbacks

        private void OnPlayButtonClicked() {
            if (_isDownloading) {
                ResetDownload();
                return;
            }

            if (_replayHeader != null) {
                _downloadButton.interactable = false;
                _playButton.interactable = false;

                LoadAndStartReplay().RunCatching();
                return;
            }

            _isWaitingToStart = true;
            StartDownload();
        }

        private void OnDownloadButtonClicked() {
            if (_isDownloading) {
                ResetDownload();
                return;
            }

            if (_replayHeader != null) {
                _replayerNavigator!.NavigateToReplayManager(_replayHeader);
                return;
            }

            _isWaitingToStart = false;
            StartDownload();
        }

        #endregion

        #region Other

        private void ResetButtons() {
            var downloadState = _replayHeader != null
                ? DownloadButtonState.ReadyToNavigate
                : _isWaitingForReplayInfos
                    ? DownloadButtonState.LoadingReplayInfos
                    : DownloadButtonState.ReadyToDownload;

            RefreshDownloadButton(downloadState);
            RefreshPlayButton(PlayButtonState.ReadyToDownloadOrStart);
        }

        private void ResetDownload() {
            _blockIncomingEvents = true;
            _isDownloading = false;
            _downloadText.gameObject.SetActive(false);

            NotifyDownloadStateChanged(false);
            ResetButtons();
        }

        private void StartDownload() {
            _blockIncomingEvents = false;
            _downloadText.gameObject.SetActive(true);

            StaticReplayRequest.Send(_score!.replay);
        }

        private static string FormatFailString(string failReason) {
            return $"<color=red>Fail: {failReason}</color>";
        }

        private void SyncReplayInfoLoadingState() {
            _isWaitingForReplayInfos = ReplayManager.IsLoading && _replayHeader == null;
        }

        #endregion

        #region Play Button

        private enum PlayButtonState {
            ReadyToDownloadOrStart,
            Downloading,
            Unavailable
        }

        private bool _playCanBeInteractable;

        private void RefreshPlayButton(PlayButtonState state) {
            if (state is PlayButtonState.Unavailable || !_playCanBeInteractable) {
                _playButton.interactable = false;
                return;
            }

            _playButton.interactable = true;
            _playButtonText.text = state switch {
                PlayButtonState.ReadyToDownloadOrStart => "<bll>ls-watch-replay</bll>",
                PlayButtonState.Downloading            => "<bll>ls-cancel</bll>",
                _                                      => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        #endregion

        #region Download Button

        private enum DownloadButtonState {
            ReadyToNavigate,
            ReadyToDownload,
            LoadingReplayInfos,
            Downloading,
            Unavailable
        }

        private void RefreshDownloadButton(DownloadButtonState state) {
            if (state is DownloadButtonState.Unavailable) {
                _downloadButton.interactable = false;
                _downloadButtonImage.Enabled = false;
                _downloadButtonSpinner.Enabled = false;
                _downloadButtonText.gameObject.SetActive(false);
                return;
            }

            var readyToDownload = state is DownloadButtonState.ReadyToDownload;
            var loadingReplayInfos = state is DownloadButtonState.LoadingReplayInfos;

            _downloadButton.interactable = state is not DownloadButtonState.LoadingReplayInfos;

            _downloadButtonImage.Enabled = readyToDownload;
            _downloadButtonSpinner.Enabled = loadingReplayInfos;
            _downloadButtonText.gameObject.SetActive(!readyToDownload && !loadingReplayInfos);

            _downloadButtonText.text = state switch {
                DownloadButtonState.ReadyToNavigate => "\u27a4",
                DownloadButtonState.ReadyToDownload => "",
                DownloadButtonState.LoadingReplayInfos => "",
                DownloadButtonState.Downloading     => "<bll>ls-cancel</bll>",
                _                                   => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        #endregion

        #region Active

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        private bool _active = true;

        public void SetActive(bool value) {
            Active = value;
            _downloadText.gameObject.SetActive(false);
        }

        #endregion
    }
}
