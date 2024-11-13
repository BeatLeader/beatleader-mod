using System;
using System.Linq;
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
using static SelectLevelCategoryViewController;

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

        private void Awake() {
            _previewPanel = Instantiate<FeaturedPreviewPanel>(transform);
        }

        #endregion

        #region Setup

        private MapDetail? _mapDetail;
        private string? _websiteUrl;
        private string? _downloadUrl;
        private bool _mapDownloaded;
        private BeatmapLevel? _map;

        private async Task FetchMap() {
            var hash = _mapDetail?.hash;
            if (hash != null) {
                _map = await ReplayerMenuLoader.Instance!.GetBeatmapLevelByHashAsync(
                    _mapDetail!.hash.ToUpper(),
                    CancellationToken.None
                );
            }
        }

        public async void Setup(MapData mapData) {
            var song = mapData.song;
            _websiteUrl = BeatSaverUtils.CreateMapPageUrl(song.id);
            _downloadUrl = song.downloadUrl;
            _mapDetail = song;
            _previewPanel.Setup(song.coverImage, song.name, song.mapper);
        }

        #endregion

        #region Callbacks

        private void OpenMap() {
            if (_map == null) return;

            var key = _map.GetBeatmapKeys().First();
            var x = new LevelSelectionFlowCoordinator.State(
				LevelCategory.All, 
				SongCore.Loader.CustomLevelsPack, 
				in key, 
				_map
			);

			FindObjectOfType<SoloFreePlayFlowCoordinator>().Setup(x);

			(GameObject.Find("SoloButton") ?? GameObject.Find("Wrapper/BeatmapWithModifiers/BeatmapSelection/EditButton"))
				?.GetComponent<NoTransitionsButton>()?.onClick.Invoke();
        }

        [UIAction("downloadPressed"), UsedImplicitly]
        private async void HandleDownloadButtonClicked() {
            await FetchMap();
            if (_map != null) {
                OpenMap();
            } else {
                _loadingContainer.SetActive(true);
                _modal.Show(true, true);
                //attempting to download
                var bytes = await WebUtils.SendRawDataRequestAsync(_downloadUrl!);
                if (bytes != null) {
                    var folderName = BeatSaverUtils.FormatBeatmapFolderName(
                        _mapDetail!.id,
                        _mapDetail.name,
                        _mapDetail.author,
                        _mapDetail.hash
                    );
                    await FileManager.InstallBeatmap(bytes, folderName);
                }
                //showing finish view
                await FetchMap();

                _mapDownloaded = _map != null;
                _finishedText.text = _map != null ? "Download has finished" : "Download has failed!";
                _loadingContainer.SetActive(false);

                OpenMap();
            }
        }

        [UIAction("ok-button-click"), UsedImplicitly]
        private void HandleOkButtonClicked() {
            _modal.Hide(true);
        }

        #endregion
    }
}