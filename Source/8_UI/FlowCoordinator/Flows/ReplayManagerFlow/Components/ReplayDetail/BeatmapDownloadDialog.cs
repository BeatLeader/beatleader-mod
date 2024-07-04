using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models.BeatSaver;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapDownloadDialog : DialogComponentBase {
        #region Tokens

        private const string GreetingsText = "Press proceed to let Monke visit BeatSaver and steal some maps for further actions.";
        private const string SearchingText = "Searching across the maps...";
        private const string SearchFailedText = "Unfortunately, Monke did not find the map.";
        private const string FoundText = "Monke just found the map on BeatSaver!";
        private const string WaitText = "Wait until Monke steal the map...";
        private const string FinishedText = "Monke successfully escaped the police so you can peacefully use the map now.";
        private const string DownloadFailedText = "Monke tried it's best but did not escape. \nAn error occured during installation!";
        private const string ForceDownloadFailedText = "Monke have tried force pull but accidentally stumbled. The version does not exist!";
        private const string ForceDownloadText = "It seems like the map has no valid version. \nAttempting to get the map from CDN...";

        private const string OkText = "Ok";
        private const string DownloadText = "Download";
        private const string ProceedText = "Proceed";

        #endregion

        #region Visuals

        private enum State {
            Greetings,
            Searching,
            SearchFailed,
            ReadyToDownload,
            Downloading,
            DownloadFailed,
            ForceDownloading,
            ForceDownloadFailed,
            Downloaded
        }

        private State _state;

        private void RefreshVisuals(State state) {
            _state = state;
            if (state is State.ReadyToDownload) {
                _mapDetailPanel.SetData(_mapDetail).ConfigureAwait(true);
            }
            //text
            _titleLabel.Text = state switch {
                State.Greetings => GreetingsText,
                State.Searching => SearchingText,
                State.SearchFailed => SearchFailedText,
                State.ReadyToDownload => FoundText,
                State.Downloading => WaitText,
                State.ForceDownloadFailed => ForceDownloadFailedText,
                State.ForceDownloading => ForceDownloadText,
                State.Downloaded => FinishedText,
                _ => DownloadFailedText
            };
            OkButtonText = state switch {
                State.Greetings => ProceedText,
                State.ReadyToDownload => DownloadText,
                _ => OkText
            };
            //visibility
            _searchIndicatorContainer.Enabled = state is State.Searching;
            _mapDetailPanel.Enabled = state is State.ReadyToDownload;
            _spinner.Enabled = state is State.Downloading or State.ForceDownloading;
            //buttons
            ShowCancelButton = state is State.Greetings or
                State.Searching or
                State.ReadyToDownload;
            ShowOkButton = state is State.Greetings or
                State.SearchFailed or
                State.ReadyToDownload or
                State.DownloadFailed or
                State.ForceDownloadFailed or
                State.Downloaded;
        }

        #endregion

        #region Construct

        private MapDetailPreviewPanel _mapDetailPanel = null!;
        private Label _titleLabel = null!;
        private Dummy _searchIndicatorContainer = null!;
        private Spinner _spinner = null!;

        protected override ILayoutItem ConstructContent() {
            return new Dummy {
                Children = {
                    new Label {
                            EnableWrapping = true
                        }
                        .AsFlexItem(size: new() { y = "auto" })
                        .Bind(ref _titleLabel),
                    //
                    new Dummy {
                        Children = {
                            new SearchIndicator {
                                Radius = 2f
                            }.AsFlexItem(margin: new() { top = 2f })
                        }
                    }.AsFlexGroup().AsFlexItem(size: new() { y = 10f }).Bind(ref _searchIndicatorContainer),
                    //
                    new MapDetailPreviewPanel()
                        .AsFlexItem(margin: new() { left = 1f, right = 1f })
                        .Bind(ref _mapDetailPanel),
                    //
                    new Spinner()
                        .AsFlexItem(size: 8f, alignSelf: Align.Center)
                        .Bind(ref _spinner)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                gap: 1f
            ).AsFlexItem(size: new() { y = 20f });
        }

        #endregion

        #region Setup

        private CancellationTokenSource _tokenSource = new();
        private MapDetail? _mapDetail;
        private string? _beatmapHash;

        public void SetHash(string hash) {
            if (IsOpened) return;
            _beatmapHash = hash;
            _mapDetail = null;
        }

        private void SearchForBeatmap() {
            SearchForBeatmapAsync(_tokenSource.Token).ConfigureAwait(true);
        }

        private void DownloadBeatmap() {
            DownloadBeatmapAsync(_tokenSource.Token).ConfigureAwait(true);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WithSizeDelta(64f, 34f);
            Content.GetOrAddComponent<CanvasGroup>().ignoreParentGroups = true;
            Title = "Download Beatmap";
        }

        #endregion

        #region Web Requests

        private async Task SearchForBeatmapAsync(CancellationToken token) {
            RefreshVisuals(State.Searching);
            //sending request
            var request = await MapDetailRequest.SendRequest(_beatmapHash!).Join();
            _mapDetail = request.Result;
            if (token.IsCancellationRequested) return;
            //
            if (_mapDetail == null) {
                RefreshVisuals(State.SearchFailed);
                return;
            }
            RefreshVisuals(State.ReadyToDownload);
        }

        private async Task DownloadBeatmapAsync(CancellationToken token) {
            if (_mapDetail == null) return;
            RefreshVisuals(State.Downloading);
            //retrieving the address
            var mapVersion = _mapDetail.versions?.FirstOrDefault(x => x.hash == _beatmapHash!.ToLower());
            var downloadUrl = mapVersion?.downloadURL;
            //downloading
            byte[]? bytes;
            if (downloadUrl == null) {
                RefreshVisuals(State.ForceDownloading);
                Plugin.Log.Warn("BeatSaver response has no valid version! Sending force request.");
                //using force download if url is not available
                var request = await DownloadMapRequest.SendRequest(_beatmapHash!).Join();
                bytes = request.Result;
            } else {
                bytes = await WebUtils.SendRawDataRequestAsync(downloadUrl);
            }
            //checking is everything ok
            if (token.IsCancellationRequested) return;
            if (bytes == null) {
                RefreshVisuals(downloadUrl == null ? State.ForceDownloadFailed : State.DownloadFailed);
                return;
            }
            //formatting folder name
            var mapMetadata = _mapDetail.metadata!;
            var folderName = BeatSaverUtils.FormatBeatmapFolderName(
                _mapDetail.id,
                mapMetadata.songName,
                mapMetadata.levelAuthorName,
                _beatmapHash
            );
            //installing the map
            if (!await FileManager.InstallBeatmap(bytes, folderName)) {
                if (token.IsCancellationRequested) return;
                RefreshVisuals(State.DownloadFailed);
                return;
            }
            if (token.IsCancellationRequested) return;
            RefreshVisuals(State.Downloaded);
            _beatmapHash = null;
        }

        #endregion

        #region Callbacks

        protected override void OnOpen(bool opened) {
            base.OnOpen(opened);
            if (opened) return;
            //
            if (_beatmapHash == null) {
                throw new UninitializedComponentException("Download dialog cannot be opened without hash");
            }
            RefreshVisuals(State.Greetings);
        }

        protected override void OnClose(bool closed) {
            base.OnClose(closed);
            if (!closed) return;
            _tokenSource.Cancel();
            _tokenSource = new();
            _beatmapHash = null;
            _mapDetail = null;
        }

        protected override void OnOkButtonClicked() {
            switch (_state) {
                case State.Greetings:
                    SearchForBeatmap();
                    break;
                case State.ReadyToDownload:
                    DownloadBeatmap();
                    break;
                case State.DownloadFailed:
                case State.SearchFailed:
                case State.ForceDownloadFailed:
                case State.Downloaded:
                    CloseInternal();
                    break;
            }
        }

        #endregion
    }
}