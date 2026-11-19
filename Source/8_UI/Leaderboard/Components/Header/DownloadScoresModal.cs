using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class DownloadScoresModal : DialogBase {
        #region Download

        public IReadOnlyCollection<IReplayHeader> Headers => _headers;
        public Action? DownloadingFinishedCallback { get; set; }

        private readonly List<Task<IWebRequest<Replay>>> _requests = new();
        private readonly List<IReplayHeader> _headers = new();

        private IReadOnlyCollection<Score>? _scores;
        private CancellationTokenSource _tokenSource = new();
        private Task? _downloadTask;

        private int _totalRequests;
        private int _currentRequests;

        public void SetData(IReadOnlyCollection<Score> scores) {
            _scores = scores;
        }

        private void StartDownloading() {
            if (_scores == null) {
                Plugin.Log.Error("Scores are null, nothing to download!");
                return;
            }

            CancelDownloading();
            _downloadTask = StartDownloadingInternal(_scores, _tokenSource.Token).RunCatching();
        }

        private void CancelDownloading() {
            if (_downloadTask == null) {
                return;
            }

            _tokenSource.Cancel();
            _tokenSource = new();
            _downloadTask = null;
        }

        private void FailDownloading(string reason) {
            CancelDownloading();
            SetPanicking(reason);
        }

        private async Task StartDownloadingInternal(IReadOnlyCollection<Score> scores, CancellationToken token) {
            _requests.Clear();
            _headers.Clear();

            if (ReplayManager.StartLoadingIfNeverLoaded()) {
                await ReplayManager.WaitForLoadingAsync();
            }

            foreach (var score in scores) {
                var header = ReplayManager.FindReplayByHash(score);

                if (header != null) {
                    _headers.Add(header);
                    continue;
                }

                var task = DownloadReplayRequest.SendRequest(score.replay, token).Join();
                _requests.Add(task);
            }

            if (token.IsCancellationRequested) {
                return;
            }

            _totalRequests = _requests.Count;
            _currentRequests = 0;

            foreach (var request in _requests) {
                var result = await request;

                if (result.RequestStatusCode != HttpStatusCode.OK) {
                    FailDownloading(result.FailReason!);
                    return;
                }

                _currentRequests++;
                RefreshDownloading();
            }

            if (token.IsCancellationRequested) {
                return;
            }

            SetSaving();
            foreach (var request in _requests) {
                var replay = request.Result.Result!;

                IReplayHeader? header;
                if (_saveReplaysToggle.Toggled) {
                    var result = await ReplayManager.SaveAnyReplayAsync(replay, null, token);

                    // For cases when id in a replay is different from the id in a score.
                    // Such thing usually happens on profile migration.
                    if (result.Error is ReplaySavingError.AlreadyExists) {
                        // Assign an existing replay
                        header = ReplayManager.FindReplayByHash(replay.info);

                        Plugin.Log.Error("[ReplayManager] Hash collision occured! The replay won't be saved");
                        Plugin.Log.Error($"Collision | player: {header!.ReplayInfo.PlayerID} | timestamp: {header.ReplayInfo.Timestamp}");
                    } else {
                        header = result.Header;
                    }
                } else {
                    header = ReplayManager.CreateTempReplayHeader(replay, null);
                }

                if (header == null) {
                    FailDownloading("Failed to save the replay");
                    return;
                }

                _headers.Add(header);
            }

            SetDownloadingFinished();

            _downloadTask = null;
        }

        #endregion

        #region Check Downloaded

        private async Task<bool> HasReplaysDownloaded() {
            if (_scores == null) {
                Plugin.Log.Error("Scores are null, nothing to check!");
                return false;
            }

            if (ReplayManager.StartLoadingIfNeverLoaded()) {
                await ReplayManager.WaitForLoadingAsync();
            }

            foreach (var score in _scores) {
                var header = ReplayManager.FindReplayByHash(score);
                
                if (header == null) {
                    return false;
                }
                
                _headers.Add(header);
            }

            return true;
        }

        private async Task CheckReplaysDownloaded() {
            SetChecking();

            var downloaded = await HasReplaysDownloaded();
            if (downloaded) {
                SetInitiallyReady();
            } else {
                SetWaitingForStart();
            }
        }

        #endregion

        #region Construct

        private Label _progressLabel = null!;
        private ProgressBar _progressBar = null!;
        private BsToggle _saveReplaysToggle = null!;
        private IReactiveComponent _saveReplaysContainer = null!;

        protected override IReactiveComponent ConstructDialogContent() {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.Center,
                    AlignItems = Align.Stretch,
                    Padding = new() { left = 3f, right = 3f },
                    Gap = new() { y = 1f }
                },

                FlexItem = {
                    MinSize = new() { x = 52.pt, y = 16.pt }
                },

                Children = {
                    new Label {
                        FlexItem = {
                            AlignSelf = Align.Center
                        },

                        Overflow = TextOverflowModes.Ellipsis
                    }.Bind(ref _progressLabel),

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            Padding = new() { bottom = 1f },
                            Gap = new() { y = 1f }
                        },

                        Children = {
                            new BsToggle()
                                .Bind(ref _saveReplaysToggle)
                                .InNamedRail("Save replays?")
                        }
                    }.Bind(ref _saveReplaysContainer),

                    new ProgressBar {
                        FlexItem = {
                            Margin = new() { left = 2f, right = 2f }
                        }
                    }.Bind(ref _progressBar)
                }
            };
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            Title = "Download Scores";
            OkButton.Text = "Proceed";
            OkButton.Interactable = false;
            OkButton.OnClick = HandleOkButtonClicked;
            CancelButton.OnClick = HandleCancelButtonClicked;
            OnPushed = HandlePushed;
        }

        private void HandlePushed() {
            _saveReplaysToggle.Toggled = false;
            CheckReplaysDownloaded().RunCatching();
        }

        private void Close() {
            _downloadingWasEverStarted = false;
            IsPushed = false;
        }

        private void HandleOkButtonClicked() {
            if (!_downloadingWasEverStarted) {
                StartDownloading();
                SetDownloading();
                RefreshDownloading();
                return;
            }

            DownloadingFinishedCallback?.Invoke();
            Close();
        }

        private void HandleCancelButtonClicked() {
            CancelDownloading();
            Close();
        }

        #endregion

        #region UI

        private bool _downloadingWasEverStarted;

        private void RefreshDownloading() {
            _progressBar.TotalProgress = _totalRequests;
            _progressBar.Progress = _currentRequests;
            _progressLabel.Text = $"Downloading {_currentRequests}/{_totalRequests}";
        }

        private void SetDownloading() {
            _saveReplaysContainer.Enabled = false;
            _progressBar.Enabled = true;
            _downloadingWasEverStarted = true;
        }

        private void SetChecking() {
            _progressLabel.Text = "Wait just a little bit...";
            _progressLabel.Color = Color.white;
            CancelButton.Interactable = false;
            OkButton.Interactable = false;
        }

        private void SetWaitingForStart() {
            _progressLabel.Color = Color.white;
            _progressLabel.Text = "Let's clarify before we start";
            _progressBar.Color = BeatSaberStyle.BsToggleColors.KnobColors.GetColor(GraphicState.Active);
            _progressBar.Progress = 0f;
            _progressBar.Enabled = false;
            _saveReplaysContainer.Enabled = true;

            CancelButton.Interactable = true;
            OkButton.Interactable = true;
            OkButton.Enabled = true;
        }

        private void SetPanicking(string reason) {
            _progressLabel.Text = $"{reason}";
            _progressLabel.Color = Color.red;
            _progressBar.Color = Color.red;
            OkButton.Interactable = false;
            OkButton.Enabled = false;
        }

        private void SetDownloadingFinished() {
            _progressLabel.Text = "Downloading finished";
            _progressBar.Progress = 1f;
            _progressBar.TotalProgress = 1f;
            _progressBar.Color = Color.green * 0.8f;
            OkButton.Interactable = true;
        }

        private void SetInitiallyReady() {
            _progressLabel.Text = "Everything is ready!";
            _progressBar.Enabled = false;
            _saveReplaysContainer.Enabled = false;
            CancelButton.Interactable = true;
            OkButton.Interactable = true;
            _downloadingWasEverStarted = true;
        }

        private void SetSaving() {
            _progressLabel.Text = "Saving replays...";
            OkButton.Interactable = false;
        }

        #endregion
    }
}