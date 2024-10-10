using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class MapPreviewPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("preview"), UsedImplicitly]
        private EventPreviewPanel _previewPanel = null!;

        [UIValue("download-button"), UsedImplicitly]
        private HeaderButton _downloadButton = null!;

        [UIValue("website-button"), UsedImplicitly]
        private HeaderButton _websiteButton = null!;

        [UIComponent("loading-modal"), UsedImplicitly]
        private ModalView _modal = null!;

        [UIObject("finished-container"), UsedImplicitly]
        private GameObject _finishedContainer = null!;

        [UIComponent("finished-text"), UsedImplicitly]
        private TMP_Text _finishedText = null!;

        [UIObject("loading-container"), UsedImplicitly]
        private GameObject _loadingContainer = null!;

        #endregion

        #region Setup

        private MapDetail? _mapDetail;
        private string? _websiteUrl;
        private string? _downloadUrl;
        private bool _mapDownloaded;

        private async Task RefreshButtons() {
            var hasMap = false;
            var hash = _mapDetail?.hash;
            if (hash != null) {
                var map = await ReplayerMenuLoader.Instance!.GetBeatmapLevelByHashAsync(
                    _mapDetail!.hash,
                    CancellationToken.None
                );
                hasMap = map != null;
            }
            _downloadButton.GetRootTransform().gameObject.SetActive(!hasMap && !_mapDownloaded && _downloadUrl != null);
            _websiteButton.GetRootTransform().gameObject.SetActive(_websiteUrl != null);
        }

        public async void Setup(MapData mapData) {
            var song = mapData.song;
            _websiteUrl = BeatSaverUtils.CreateMapPageUrl(song.id);
            _downloadUrl = song.downloadUrl;
            _mapDetail = song;
            _previewPanel.Setup(song.coverImage, song.name, song.mapper);
            await RefreshButtons();
        }

        protected override async void OnInstantiate() {
            _previewPanel = Instantiate<EventPreviewPanel>(transform);
            _downloadButton = Instantiate<HeaderButton>(transform);
            _websiteButton = Instantiate<HeaderButton>(transform);
            _downloadButton.Setup(BundleLoader.SaveIcon);
            _downloadButton.OnClick += HandleDownloadButtonClicked;
            _websiteButton.Setup(BundleLoader.ProfileIcon);
            _websiteButton.OnClick += HandleWebsiteButtonClicked;
            await RefreshButtons();
        }

        #endregion

        #region Callbacks

        private void HandleWebsiteButtonClicked() {
            EnvironmentUtils.OpenBrowserPage(_websiteUrl!);
        }

        private async void HandleDownloadButtonClicked() {
            _loadingContainer.SetActive(true);
            _finishedContainer.SetActive(false);
            _modal.Show(true, true);
            //attempting to download
            var result = false;
            var bytes = await WebUtils.SendRawDataRequestAsync(_downloadUrl!);
            if (bytes != null) {
                var folderName = BeatSaverUtils.FormatBeatmapFolderName(
                    _mapDetail!.id,
                    _mapDetail.name,
                    _mapDetail.author,
                    _mapDetail.hash
                );
                result = await FileManager.InstallBeatmap(bytes, folderName);
            }
            //showing finish view
            _mapDownloaded = result;
            _finishedText.text = result ? "Download has finished" : "Download has failed!";
            _loadingContainer.SetActive(false);
            _finishedContainer.SetActive(true);
            await RefreshButtons();
        }

        [UIAction("ok-button-click"), UsedImplicitly]
        private void HandleOkButtonClicked() {
            _modal.Hide(true);
        }

        #endregion
    }
}