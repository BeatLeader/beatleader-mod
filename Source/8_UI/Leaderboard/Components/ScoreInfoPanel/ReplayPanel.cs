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
        private bool _blockedUntilLoaded;

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

            StaticReplayRequest.ProgressChangedEvent += OnDownloadProgressChanged;
            StaticReplayRequest.StateChangedEvent += OnDownloadRequestStateChanged;

            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        protected override void OnDispose() {
            StaticReplayRequest.ProgressChangedEvent -= OnDownloadProgressChanged;
            StaticReplayRequest.StateChangedEvent -= OnDownloadRequestStateChanged;

            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        protected override void OnRootStateChange(bool active) {
            if (active && !_blockedUntilLoaded) {
                var neverLoaded = ReplayManager.StartLoadingIfNeverLoaded();

                if (neverLoaded) {
                    _blockedUntilLoaded = true;
                    BlockUntilLoaded().RunCatching();
                }
            }
        }

        private async Task BlockUntilLoaded() {
            RefreshDownloadButton(DownloadButtonState.Unavailable);
            RefreshPlayButton(PlayButtonState.Unavailable);

            _blockIncomingEvents = true;
            await ReplayManager.WaitForLoadingAsync();

            _blockIncomingEvents = false;
            _blockedUntilLoaded = false;

            if (_score != null) {
                SetScore(_score);
            }
        }

        #endregion

        #region SetScore

        private Score? _score;
        private IReplayHeader? _replayHeader;

        public void SetScore(Score score) {
            _score = score;

            if (_blockedUntilLoaded) {
                return;
            }

            _replayHeader = ReplayManager.FindReplayByHash(_score);
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
            RefreshDownloadButton(_replayHeader != null ? DownloadButtonState.ReadyToNavigate : DownloadButtonState.ReadyToDownload);
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
            Downloading,
            Unavailable
        }

        private void RefreshDownloadButton(DownloadButtonState state) {
            if (state is DownloadButtonState.Unavailable) {
                _downloadButton.interactable = false;
                return;
            }

            _downloadButton.interactable = true;

            var readyToDownload = state is DownloadButtonState.ReadyToDownload;
            _downloadButtonImage.Enabled = readyToDownload;
            _downloadButtonText.gameObject.SetActive(!readyToDownload);

            _downloadButtonText.text = state switch {
                DownloadButtonState.ReadyToNavigate => "\u27a4",
                DownloadButtonState.ReadyToDownload => "",
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