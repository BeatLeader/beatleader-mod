using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace BeatLeader.UI.MainMenu {
    internal class MapPreviewDialog : AbstractReeModal<TrendingMapData> {
        #region Components

        private SongPreviewPlayer _songPreviewPlayer = null!;
        private AudioClip? _previewClip;
        private CancellationTokenSource? _downloadCancellationSource;

        [UIObject("loading-container"), UsedImplicitly] private GameObject _loadingContainer = null!;
        [UIObject("finished-container"), UsedImplicitly] private GameObject _finishedContainer = null!;
        [UIComponent("finished-text"), UsedImplicitly] private TMP_Text _finishedText = null!;
        [UIComponent("cover-image"), UsedImplicitly] private Image _coverImage = null!;
        [UIComponent("play-button"), UsedImplicitly] private Button _playButton = null!;

        private string _mapName = string.Empty;
        [UIValue("map-name"), UsedImplicitly]
        private string MapName {
            get => _mapName;
            set {
                if (_mapName.Equals(value)) return;
                _mapName = value;
                NotifyPropertyChanged();
            }
        }

        private string _songAuthor = string.Empty;
        [UIValue("song-author"), UsedImplicitly]
        private string SongAuthor {
            get => _songAuthor;
            set {
                if (_songAuthor.Equals(value)) return;
                _songAuthor = value;
                NotifyPropertyChanged();
            }
        }

        private string _mapper = string.Empty;
        [UIValue("mapper"), UsedImplicitly]
        private string Mapper {
            get => _mapper;
            set {
                if (_mapper.Equals(value)) return;
                _mapper = value;
                NotifyPropertyChanged();
            }
        }

        private string _description = string.Empty;
        [UIValue("description"), UsedImplicitly]
        private string Description {
            get => _description;
            set {
                if (_description.Equals(value)) return;
                _description = value;
                NotifyPropertyChanged();
            }
        }

        private string _trendingValue = string.Empty;
        [UIValue("trending-value"), UsedImplicitly]
        private string TrendingValue {
            get => _trendingValue;
            set {
                if (_trendingValue.Equals(value)) return;
                _trendingValue = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region OpenSongOrDownloadDialog

        public static async void OpenSongOrDownloadDialog(TrendingMapData mapDetail, Transform screenChild) {
            BeatmapLevel? map = null; 
            try {    
                map = await FetchMap(mapDetail.song);
            } catch (Exception) { }

            if (map != null) {
                OpenMap(map, null);
            } else {
                ReeModalSystem.OpenModal<MapPreviewDialog>(screenChild, mapDetail);
            }
        }

        private static async Task<BeatmapLevel?> FetchMap(MapDetail mapDetail) {
            BeatmapLevel? map = null;

            var hash = mapDetail.hash;
            if (hash != null) {
                map = await ReplayerMenuLoader.Instance!.GetBeatmapLevelByHashAsync(
                    hash.ToUpper(),
                    CancellationToken.None
                );
            }

            return map;
        }

        private static void OpenMap(BeatmapLevel? map, MapPreviewDialog? dialog) {
            if (map == null) return;

            var key = map.GetBeatmapKeys().First();
            var x = new LevelSelectionFlowCoordinator.State(
                SelectLevelCategoryViewController.LevelCategory.All,
                SongCore.Loader.CustomLevelsPack,
                in key,
                map
            );

            FindObjectOfType<SoloFreePlayFlowCoordinator>().Setup(x);

            if (dialog != null) {
                dialog.Close();
            }

            (GameObject.Find("SoloButton") ?? GameObject.Find("Wrapper/BeatmapWithModifiers/BeatmapSelection/EditButton"))
                ?.GetComponent<NoTransitionsButton>()?.onClick.Invoke();
        }

        #endregion

        #region Events

        private bool _mapDownloaded;

        protected override void OnContextChanged() {
            _songPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();
            
            MapName = Context.song.name;
            SongAuthor = Context.song.author;
            Mapper = $"Mapped by {Context.song.mapper}";
            Description = Context.description;
            TrendingValue = Context.trendingValue;
            
            // Load cover image
            if (!string.IsNullOrEmpty(Context.song.coverImage)) {
                _ = LoadCoverImage();
            }
        }

        private async Task LoadCoverImage() {
            try {
                await _coverImage.SetImageAsync(Context.song.coverImage!);
            } catch (Exception) {
                // Ignore cover loading errors
            }
        }

        private async Task LoadAndPlayPreview(CancellationToken cancellationToken) {
            try {
                var previewUrl = $"https://eu.cdn.beatsaver.com/{Context.song.hash.ToLowerInvariant()}.mp3";
                
                using (var www = UnityWebRequestMultimedia.GetAudioClip(previewUrl, AudioType.MPEG)) {
                    var operation = www.SendWebRequest();

                    while (!operation.isDone) {
                        if (cancellationToken.IsCancellationRequested) {
                            www.Abort();
                            return;
                        }
                        await Task.Delay(50);
                    }

                    if (www.result == UnityWebRequest.Result.Success) {
                        _previewClip = DownloadHandlerAudioClip.GetContent(www);
                        if (_previewClip != null && _songPreviewPlayer != null) {
                            _songPreviewPlayer.CrossfadeTo(_previewClip, 0, 1, _previewClip.length, null);
                        }
                    }
                }
            } catch (Exception) {
                // Ignore preview loading errors
            }
        }

        protected override void OnResume() {
            _playButton.interactable = true;
            _loadingContainer.SetActive(false);
            _finishedContainer.SetActive(true);
            _finishedText.text = "";
            offClickCloses = true;

            // Load and play preview
            if (!string.IsNullOrEmpty(Context.song.hash)) {
                _downloadCancellationSource?.Cancel();
                _downloadCancellationSource = new CancellationTokenSource();
                _ = LoadAndPlayPreview(_downloadCancellationSource.Token);
            }
        }

        [UIAction("play-button-click"), UsedImplicitly]
        private async void HandlePlayButtonClicked() {
            _playButton.interactable = false;
            _loadingContainer.SetActive(true);
            _finishedContainer.SetActive(false);
            offClickCloses = false;

            await Download();
        }

        [UIAction("ok-button-click"), UsedImplicitly]
        private void HandleOkButtonClicked() {
            Close();
        }

        private async Task Download() {
            try {
                var bytes = await WebUtils.SendRawDataRequestAsync(Context.song.downloadUrl!);
                if (bytes != null) {
                    var basePath = Context.song.id + " (" + Context.song.name + " - " + Context.song.author + ")";
                    basePath = string.Join("", basePath.Split(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()));
                    await FileManager.InstallBeatmap(bytes, basePath);
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    var map = await FetchMap(Context.song);
                    _mapDownloaded = map != null;

                    if (_mapDownloaded) {
                        OpenMap(map, this);
                        return;
                    }
                }
            } catch (Exception) {
                _mapDownloaded = false;
            }

            // If we get here, something went wrong
            _finishedText.text = "Download failed!";
            _loadingContainer.SetActive(false);
            _finishedContainer.SetActive(true);
            _playButton.interactable = true;
            offClickCloses = true;
        }

        protected override void OnClose() {
            _downloadCancellationSource?.Cancel();
            _downloadCancellationSource?.Dispose();
            _downloadCancellationSource = null;

            if (_songPreviewPlayer != null) {
                _songPreviewPlayer.CrossfadeToDefault();
            }
        }

        #endregion
    }
}