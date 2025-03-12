using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.UI;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class DownloadScoresModal : DialogComponentBase {
        #region Download

        public IReadOnlyCollection<IReplayHeader> Headers => _headers;
        public Action? DownloadingFinishedCallback { get; set; }

        private readonly List<IWebRequest<Replay>> _requests = new();
        private readonly List<IReplayHeader> _headers = new();

        private CancellationTokenSource _tokenSource = new();
        private Task? _downloadTask;

        private int _totalRequests;
        private int _currentRequests;

        public void StartDownloading(IReadOnlyCollection<Score> scores) {
            CancelDownloading();
            _downloadTask = StartDownloadingInternal(scores, _tokenSource.Token);
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
            
            foreach (var score in scores) {
                var header = FindReplayHeader(score);
                if (header != null) {
                    _headers.Add(header);
                    continue;
                }

                var task = DownloadReplayRequest.SendRequest(score.replay, token);
                _requests.Add(task);
            }

            _totalRequests = _requests.Count;
            _currentRequests = 0;

            foreach (var request in _requests) {
                var req = await request.Join();

                if (req.RequestStatusCode != HttpStatusCode.OK) {
                    FailDownloading(req.FailReason!);
                    return;
                }

                _currentRequests++;
                RefreshDownloading();
            }

            SetSaving();
            foreach (var request in _requests) {
                var header = ReplayManager.Instance.SaveAnyReplay(request.Result!, null);

                if (header == null) {
                    FailDownloading("Failed to save the replay");
                    return;
                }

                _headers.Add(header);
            }

            SetFinished();
            _downloadTask = null;
        }

        // TODO: move to ReplayManager and add hashed access
        private static IReplayHeader? FindReplayHeader(Score score) {
            if (!long.TryParse(score.timeSet, out var scoreTimestamp)) {
                Plugin.Log.Error($"Failed to parse score's timestamp: {score.timeSet}");
                return null;
            }

            return ReplayManager.Instance.Replays
                .Where(x => x.ReplayInfo.SongHash == LeaderboardState.SelectedBeatmapKey.levelId)
                .Where(x => x.ReplayInfo.Timestamp == scoreTimestamp)
                .FirstOrDefault(x => x.ReplayInfo.PlayerID == score.ActualPlayer.id);
        }

        #endregion

        #region Construct

        private Label _progressLabel = null!;
        private ProgressBar _progressBar = null!;

        protected override ILayoutItem ConstructContent() {
            return new Dummy {
                Children = {
                    new Label {
                        Overflow = TextOverflowModes.Ellipsis
                    }.AsFlexItem(
                        size: "auto",
                        alignSelf: Align.Center
                    ).Bind(ref _progressLabel),

                    new ProgressBar()
                        .AsFlexItem(margin: new() { left = 2f, right = 2f })
                        .Bind(ref _progressBar)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                alignItems: Align.Stretch
            );
        }

        protected override void OnInitialize() {
            OkButtonInteractable = false;
            OkButtonText = "Proceed";
            Title = "Download Scores";

            Content.GetOrAddComponent<CanvasGroup>().ignoreParentGroups = true;

            this.WithSizeDelta(40f, 30f);
            base.OnInitialize();
        }

        protected override void OnOpen(bool opened) {
            if (opened) {
                return;
            }
            
            _progressLabel.Color = Color.white;
            _progressLabel.Text = "Waiting for download...";
            _progressBar.Color = UIStyle.ControlButtonColorSet.ActiveColor;
            _progressBar.Progress = 0f;
            OkButtonInteractable = false;
        }

        protected override void OnOkButtonClicked() {
            DownloadingFinishedCallback?.Invoke();
            CloseInternal();
        }

        #endregion

        #region UI

        private void RefreshDownloading() {
            _progressBar.TotalProgress = _totalRequests;
            _progressBar.Progress = _currentRequests;
            _progressLabel.Text = $"Downloading {_currentRequests}/{_totalRequests}";
        }

        private void SetPanicking(string reason) {
            _progressLabel.Text = $"{reason}";
            _progressLabel.Color = Color.red;
            _progressBar.Color = Color.red;
        }

        private void SetFinished() {
            _progressLabel.Text = "Downloading finished";
            _progressBar.Progress = 1f;
            _progressBar.TotalProgress = 1f;
            _progressBar.Color = Color.green * 0.8f;
            OkButtonInteractable = true;
            CancelButtonInteractable = true;
        }

        private void SetSaving() {
            _progressLabel.Text = "Saving replays...";
            CancelButtonInteractable = false;
        }

        #endregion
    }
}