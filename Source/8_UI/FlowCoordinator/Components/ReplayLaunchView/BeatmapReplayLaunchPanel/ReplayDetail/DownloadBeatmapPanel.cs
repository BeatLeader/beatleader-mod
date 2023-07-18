using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.BeatSaver;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class DownloadBeatmapPanel : ReeUIComponentV2 {
        #region Configuration

        private const string HelpText = "<size=3>* Press 'Download Map' button again to proceed \n* Press on the song to open it in browser</size>";
        private const string WaitText = "Wait a little...";
        private const string FinishedText = "<size=3>The map is installed! \nPress 'Back' to continue</size>";
        private const string FailedText = "<size=3>An error occured during installation! \nPress 'Back' to continue</size>";
        private const string InvalidText = "<size=2.7>Oops.. Force request failed, the version does not exist! \nPress 'Back' to continue</size>";
        private const string ForceText = "<size=3>It seems like the map has no valid version! \nAttempting to get the map from CDN...</size>";

        private const string FailedPanelText = "Sadly, Monke cannot find the map!";
        private const string IdlingPanelText = "Monke has no work to do!";

        #endregion

        #region UI Components

        [UIValue("search-indicator"), UsedImplicitly]
        private SearchIndicator _searchIndicator = null!;

        [UIComponent("back-button"), UsedImplicitly]
        private Button _backButton = null!;

        [UIComponent("back-button"), UsedImplicitly]
        private Transform _backButtonTransform = null!;

        [UIComponent("download-info-text"), UsedImplicitly]
        private TMP_Text _downloadInfoText = null!;

        [UIComponent("map-preview-image"), UsedImplicitly]
        private Image _mapPreviewImage = null!;

        [UIComponent("map-text"), UsedImplicitly]
        private TMP_Text _mapText = null!;

        [UIComponent("map-bsr-text"), UsedImplicitly]
        private TMP_Text _mapBsrText = null!;

        [UIComponent("info-panel-text"), UsedImplicitly]
        private TMP_Text _infoPanelText = null!;

        [UIObject("info-panel"), UsedImplicitly]
        private GameObject _infoPanelObject = null!;

        [UIObject("searching-panel"), UsedImplicitly]
        private GameObject _searchingPanelObject = null!;

        [UIObject("download-panel"), UsedImplicitly]
        private GameObject _downloadPanelObject = null!;

        #endregion

        #region Events

        public event Action? BackButtonClickedEvent;
        public event Action<bool>? DownloadAbilityChangedEvent;

        #endregion

        #region BackText

        [UIValue("back-button-text"), UsedImplicitly]
        public string BackText {
            get => _backText;
            set {
                _backText = value;
                NotifyPropertyChanged();
            }
        }

        private string _backText = "Back";

        #endregion

        #region UpdateVisibility

        private enum State {
            Idling,
            Searching,
            SearchFailed,
            ReadyToDownload,
            Downloading,
            DownloadFailed,
            DownloadInvalid,
            DownloadForce,
            Downloaded
        }

        private void UpdateVisibility(State state) {
            _infoPanelObject.SetActive(false);
            _searchingPanelObject.SetActive(false);
            _downloadPanelObject.SetActive(false);

            switch (state) {
                case State.Idling:
                    _infoPanelText.text = IdlingPanelText;
                    _infoPanelObject.SetActive(true);
                    break;
                case State.Searching:
                    _searchingPanelObject.SetActive(true);
                    break;
                case State.SearchFailed:
                    _infoPanelText.text = FailedPanelText;
                    _infoPanelObject.SetActive(true);
                    break;
                case State.ReadyToDownload:
                    _mapPreviewImage.SetImage(_mapDetail!.versions![0].coverURL);
                    _mapText.text = "By " + _mapDetail.metadata!.levelAuthorName;
                    _mapBsrText.text = "BSR: " + _mapDetail.id;
                    _downloadInfoText.text = HelpText;
                    _downloadPanelObject.SetActive(true);
                    break;
                case State.Downloading:
                case State.DownloadFailed:
                case State.DownloadInvalid:
                case State.DownloadForce:
                case State.Downloaded:
                    _downloadInfoText.text = state switch {
                        State.Downloading => WaitText,
                        State.DownloadInvalid => InvalidText,
                        State.DownloadForce => ForceText,
                        State.Downloaded => FinishedText,
                        _ => FailedText
                    };
                    _downloadPanelObject.SetActive(true);
                    break;
            }
        }

        #endregion

        #region Init

        protected override void OnRootStateChange(bool active) {
            if (_beatmapHash is null) {
                UpdateVisibility(State.Idling);
                return;
            }
            if (_mapDetail is not null) {
                UpdateVisibility(State.ReadyToDownload);
                return;
            }
            if (!active) return;
            _ = GetBeatmapAsync(_cancellationTokenSource.Token);
        }

        protected override void OnInstantiate() {
            _searchIndicator = Instantiate<SearchIndicator>(transform);
            _searchIndicator.radius = 2;
            _searchIndicator.speed = -3;
        }

        protected override void OnInitialize() {
            _backButtonTransform.Find("Underline")?.gameObject.SetActive(false);
            _mapPreviewImage.material = Resources
                .FindObjectsOfTypeAll<Material>()
                .FirstOrDefault(x => x.name == "UINoGlowRoundEdge");
        }

        #endregion

        #region Web Requests

        private CancellationTokenSource _cancellationTokenSource = new();
        private MapDetail? _mapDetail;
        private string? _beatmapHash;

        public void SetHash(string hash) {
            if (!_backButton.interactable) {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new();
            }
            _beatmapHash = hash;
            _mapDetail = null;
        }

        private async Task GetBeatmapAsync(CancellationToken token) {
            _backButton.interactable = false;
            DownloadAbilityChangedEvent?.Invoke(false);
            UpdateVisibility(State.Searching);

            _mapDetail = await BeatSaverUtils.GetMapByHashAsync(_beatmapHash!);
            if (token.IsCancellationRequested) return;

            _backButton.interactable = true;
            if (_mapDetail is null) {
                UpdateVisibility(State.SearchFailed);
                return;
            }
            DownloadAbilityChangedEvent?.Invoke(true);
            UpdateVisibility(State.ReadyToDownload);
        }

        private async Task DownloadBeatmapAsync(CancellationToken token) {
            if (_mapDetail is null) return;
            _backButton.interactable = false;
            DownloadAbilityChangedEvent?.Invoke(false);
            UpdateVisibility(State.Downloading);
            //download the map
            var bytes = default(byte[]?);
            var downloadUrl = _mapDetail.versions?.FirstOrDefault(
                x => x.hash == _beatmapHash!.ToLower())?.downloadURL;
            if (downloadUrl is null) {
                UpdateVisibility(State.DownloadForce);
                var url = BeatSaverUtils.CreateDownloadMapUrl(_beatmapHash!);
                Plugin.Log.Warn("BeatSaver response has no valid version! Sending force request to " + url);
                bytes = await WebUtils.SendRawDataRequestAsync(url);
            } else {
                bytes = await WebUtils.SendRawDataRequestAsync(downloadUrl);
            }
            //check is everything ok
            if (token.IsCancellationRequested) return;
            if (bytes is null) {
                UpdateVisibility(downloadUrl is null ? State.DownloadInvalid : State.DownloadFailed);
                _backButton.interactable = true;
                return;
            }
            //save the map
            var mapMetadata = _mapDetail.metadata!;
            if (!await FileManager.InstallBeatmap(bytes, BeatSaverUtils.FormatBeatmapFolderName(
                _mapDetail.id, mapMetadata.songName, mapMetadata.levelAuthorName, _beatmapHash))) {
                UpdateVisibility(State.DownloadFailed);
                _backButton.interactable = true;
                return;
            }
            if (token.IsCancellationRequested) return;
            UpdateVisibility(State.Downloaded);
            _backButton.interactable = true;
            _beatmapHash = null;
        }

        #endregion

        #region Callbacks

        public void NotifyDownloadButtonClicked() {
            if (_mapDetail is null) return;
            _ = DownloadBeatmapAsync(_cancellationTokenSource.Token);
        }

        [UIAction("open-link-button-click"), UsedImplicitly]
        private void HandleOpenLinkButtonClicked() {
            EnvironmentUtils.OpenBrowserPage(BeatSaverUtils.CreateMapPageUrl(_mapDetail!.id!));
        }

        [UIAction("back-button-click"), UsedImplicitly]
        private void HandleBackButtonClicked() {
            BackButtonClickedEvent?.Invoke();
        }

        #endregion
    }
}