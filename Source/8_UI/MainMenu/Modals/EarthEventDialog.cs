using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class EarthEventDialog : AbstractReeModal<object> {
        #region Setup

        [UIComponent("generatebutton")]
        private TMP_Text _generateButtonText = null!;

        [UIComponent("generatebutton")]
        private Button _generateButton = null!;

        [UIComponent("playbutton")]
        private TMP_Text _playButtonText = null!;

        [UIComponent("playbutton")]
        private Button _playButton = null!;

        private EarthDayMap? earthDayMap = null;

        private bool _downloadingMap;

        protected override void OnResume() {
            EarthDayRequest.AddStateListener(OnRequestStateChanged);
            EarthDayRequest.SendRequest(ProfileManager.Profile.id);
        }

        protected override void OnClose() {
            EarthDayRequest.RemoveStateListener(OnRequestStateChanged);
        }

        private void OnRequestStateChanged(API.RequestState state, EarthDayMap result, string failReason) {
            if (state == API.RequestState.Finished) {
                earthDayMap = result;
                if (_downloadingMap) {
                    PlayMap();
                } else {
                    RefreshMap();
                }
            } else if (state == API.RequestState.Failed) {
                earthDayMap = null;
                RefreshMap();
            }
        }

        private void RefreshMap() {
            if (earthDayMap != null) {
                _generateButtonText.text = "Re-generate";
                _playButtonText.text = "Play";
                _playButton.gameObject.SetActive(true);
            } else {
                _generateButtonText.text = "Generate!";
                _playButton.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Events

        private static async Task<BeatmapLevel?> FetchMap(string hash) {
            BeatmapLevel? map = null;
            map = await ReplayerMenuLoader.Instance!.GetBeatmapLevelByHashAsync(
                hash.ToUpper(),
                CancellationToken.None
            );

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

        private async Task PlayMap() {
            var map = await FetchMap(earthDayMap.hash);

            if (map != null) {
                OpenMap(map, null);
            }
        }

        [UIAction("button-click"), UsedImplicitly]
        private async void OnButtonClick() {
            _downloadingMap = true;
            
            _generateButton.interactable = false;
            _generateButtonText.text = "Working...";

            try {
                var result = await WebUtils.SendRawDataRequestAsync($"{BLConstants.BEATLEADER_API_URL}/earthday/generate");

                if (result == null) {
                    throw new Exception("Failed to make a map =(");
                }

                await FileManager.InstallBeatmap(result, "EarthDayMap");
            } catch (Exception e) {
                Plugin.Log.Critical(e);
            }

            _generateButton.interactable = true;
            EarthDayRequest.SendRequest(ProfileManager.Profile.id);
        }

        [UIAction("play-click"), UsedImplicitly]
        private async void OnPlayClick() {
            var mapDownloaded = SongCore.Collections.songWithHashPresent(earthDayMap.hash.ToUpper());
            if (mapDownloaded) {
                await PlayMap();
            } else {
                try {
                    var result = await WebUtils.SendRawDataRequestAsync($"https://cdn.songs.beatleader.com/EarthDayMap_{earthDayMap.playerId}_{earthDayMap.timeset}.zip");

                    if (result == null) {
                        throw new Exception("Failed to make a map =(");
                    }

                    await FileManager.InstallBeatmap(result, "EarthDayMap");
                    await PlayMap();
                } catch (Exception e) {
                    Plugin.Log.Critical(e);
                }
            }
        }

        #endregion
    }
}