using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplayDetailPanel : BasicReplayDetailPanel {
        #region Configuration

        private const string WatchTextToken = "ls-watch-replay-short";
        private const string DownloadTextToken = "ls-download-map";

        #endregion

        #region Construct

        private ReplayDeletionDialog _replayDeletionDialog = null!;
        private BeatmapDownloadDialog _beatmapDownloadDialog = null!;
        private LoadingContainer _watchButtonContainer = null!;
        private ButtonBase _watchButton = null!;
        private Label _watchButtonLabel = null!;

        protected override ILayoutItem ConstructButtons() {
            return new Dummy {
                Children = {
                    new BeatmapDownloadDialog()
                        .WithCloseListener(HandleDownloadBeatmapDialogClosed)
                        .WithAnchor(() => (RectTransform)Canvas!.transform, RelativePlacement.Center)
                        .WithAlphaAnimation(() => Canvas!.gameObject)
                        .WithJumpAnimation()
                        .Bind(ref _beatmapDownloadDialog),
                    //
                    new ReplayDeletionDialog()
                        .WithAnchor(() => (RectTransform)Canvas!.transform, RelativePlacement.Center)
                        .WithAlphaAnimation(() => Canvas!.gameObject)
                        .WithJumpAnimation()
                        .Bind(ref _replayDeletionDialog),
                    //
                    new BsButton {
                            OnClick = HandleDeleteButtonClicked
                        }
                        .WithLocalizedLabel("ls-delete")
                        .AsFlexItem(grow: 1f),
                    //
                    new BsPrimaryButton {
                            OnClick = HandleWatchButtonClicked
                        }
                        .WithLocalizedLabel(out _watchButtonLabel, WatchTextToken)
                        .AsFlexItem(grow: 1f)
                        .Bind(ref _watchButton)
                        .InLoadingContainer()
                        .Bind(ref _watchButtonContainer)
                }
            }.AsFlexGroup(gap: 2f).AsFlexItem(size: new() { y = 8f });
        }

        protected override bool AllowTagsEdit => true;
        protected override string EmptyText => "Select a replay to let Monke think on it";

        #endregion

        #region Setup

        private ReplayerMenuLoader? _replayerMenuLoader;

        public void Setup(ReplayerMenuLoader menuLoader, IReplayTagManager tagManager) {
            _replayerMenuLoader = menuLoader;
            SetupInternal(tagManager);
        }

        #endregion

        #region Data

        private bool _needToDownloadBeatmap;

        protected override async Task SetDataInternalAsync(IReplayHeader header, CancellationToken token) {
            var info = header.ReplayInfo;
            _watchButtonContainer.Loading = true;
            var beatmap = await _replayerMenuLoader!.LoadBeatmapAsync(
                info.SongHash,
                info.SongMode,
                info.SongDifficulty,
                token
            );
            _needToDownloadBeatmap = !beatmap.HasValue;
            _watchButtonLabel.SetLocalizedText(_needToDownloadBeatmap ? DownloadTextToken : WatchTextToken);
            _watchButtonContainer.Loading = false;
            _watchButton.Interactable = _needToDownloadBeatmap || SongCoreInterop.ValidateRequirements(beatmap!);
        }

        #endregion

        #region Callbacks

        private void HandleReplayLoadingFinished() {
            _watchButton.Interactable = true;
        }
        
        private void HandleDownloadBeatmapDialogClosed(IModal modal, bool closed) {
            if (closed) return;
            SetDataInternalAsync(Header!, CancellationToken).ConfigureAwait(true);
        }

        private void HandleDeleteButtonClicked() {
            _replayDeletionDialog.SetHeader(Header!);
            _replayDeletionDialog.Present(ContentTransform);
        }

        private async void HandleWatchButtonClicked() {
            if (Header == null) {
                throw new UninitializedComponentException("Replay header was null");
            }
            if (_needToDownloadBeatmap) {
                _beatmapDownloadDialog.SetHash(Header.ReplayInfo.SongHash);
                _beatmapDownloadDialog.Present(ContentTransform);
                return;
            }
            _watchButton.Interactable = false;
            var replay = await Header.LoadReplayAsync(CancellationToken.None);
            var player = await Header.LoadPlayerAsync(false, CancellationToken.None);
            await _replayerMenuLoader!.StartReplayAsync(replay!, player, finishCallback: HandleReplayLoadingFinished);
        }

        #endregion
    }
}