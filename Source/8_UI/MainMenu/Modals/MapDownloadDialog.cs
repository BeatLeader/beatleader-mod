using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class MapDownloadDialog : AbstractReeModal<MapDetail> {
        #region Components

        [UIObject("loading-container"), UsedImplicitly] private GameObject _loadingContainer = null!;

        [UIObject("finished-container"), UsedImplicitly] private GameObject _finishedContainer = null!;

        [UIComponent("finished-text"), UsedImplicitly] private TMP_Text _finishedText = null!;

        private bool _okButtonActive;

        [UIValue("ok-button-active"), UsedImplicitly]
        private bool OkButtonActive {
            get => _okButtonActive;
            set {
                if (_okButtonActive.Equals(value)) return;
                _okButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region OpenSongOrDownloadDialog //TODO: Move to static utils class?

        public static async void OpenSongOrDownloadDialog(MapDetail mapDetail, Transform screenChild) {
            var map = await FetchMap(mapDetail);

            if (map != null) {
                OpenMap(map, null);
            } else {
                ReeModalSystem.OpenModal<MapDownloadDialog>(screenChild, mapDetail);
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

        private static void OpenMap(BeatmapLevel? map, MapDownloadDialog? dialog) {
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

        private string _websiteUrl = string.Empty;
        private bool _mapDownloaded;

        protected override void OnContextChanged() {
            _websiteUrl = BeatSaverUtils.CreateMapPageUrl(Context.id); //TODO: Not used
        }

        protected override void OnResume() {
            _loadingContainer.SetActive(true);
            _finishedContainer.SetActive(false);
            offClickCloses = false;
            Download();
        }

        [UIAction("ok-button-click"), UsedImplicitly]
        private void HandleOkButtonClicked() {
            Close();
        }

        private async void Download() {
            //attempting to download
            var request = await RawDataRequest.Send(Context.downloadUrl!).Join();

            if (request.RequestStatusCode is not HttpStatusCode.OK) {
                return;
            }
            
            var basePath = Context.id + " (" + Context.name + " - " + Context.author + ")";
            basePath = string.Join("", basePath.Split(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()));
            
            await FileManager.InstallBeatmap(request.Result!, basePath);
            await Task.Delay(TimeSpan.FromSeconds(2));

            //showing the finish view
            var map = await FetchMap(Context);

            offClickCloses = true;
            _mapDownloaded = map != null;
            _finishedText.text = _mapDownloaded ? "Download has finished" : "Download has failed!";
            _loadingContainer.SetActive(false);
            _finishedContainer.SetActive(true);

            if (_mapDownloaded) {
                Close();
                OpenMap(map, this);
            } else {
                OkButtonActive = true;
            }
        }

        #endregion
    }
}