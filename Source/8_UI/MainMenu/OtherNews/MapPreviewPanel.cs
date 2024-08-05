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
        private FeaturedPreviewPanel _previewPanel = null!;

        [UIComponent("loading-modal"), UsedImplicitly]
        private ModalView _modal = null!;

        [UIObject("finished-container"), UsedImplicitly]
        private GameObject _finishedContainer = null!;

        [UIComponent("finished-text"), UsedImplicitly]
        private TMP_Text _finishedText = null!;

        [UIObject("loading-container"), UsedImplicitly]
        private GameObject _loadingContainer = null!;

        private bool _downloadInteractable = false;

        [UIValue("downloadInteractable")]
        public bool DownloadInteractable
        {
            get => _downloadInteractable;
            set
            {
                _downloadInteractable = value;
                NotifyPropertyChanged();
            }
        }

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
            DownloadInteractable = !hasMap && !_mapDownloaded && _downloadUrl != null;
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
            _previewPanel = Instantiate<FeaturedPreviewPanel>(transform);
            await RefreshButtons();
        }

        #endregion

        #region Callbacks

        [UIAction("downloadPressed"), UsedImplicitly]
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