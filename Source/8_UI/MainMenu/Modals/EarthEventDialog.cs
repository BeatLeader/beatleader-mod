using System;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class EarthEventDialog : AbstractReeModal<object> {
        private const string SongHash = "c8a8b50f33fe09c7716a64fe4296a862df955df9";
        private const string DownloadUrl = "https://r2cdn.beatsaver.com/c8a8b50f33fe09c7716a64fe4296a862df955df9.zip";

        #region Setup

        [UIComponent("button")]
        private TMP_Text _buttonText = null!;

        [UIComponent("button")]
        private Button _button = null!;

        private bool _mapDownloaded;
        private bool _downloadingMap;

        protected override void OnResume() {
            RefreshMap();
        }

        protected override void OnOffClick() {
            if (!_downloadingMap) {
                base.OnOffClick();
            }
        }

        private void RefreshMap() {
            _mapDownloaded = SongCore.Collections.songWithHashPresent(SongHash);
            _buttonText.text = _mapDownloaded ? "Play" : "Download";
        }

        #endregion

        #region Events

        [UIAction("button-click"), UsedImplicitly]
        private async void OnButtonClick() {
            _downloadingMap = true;
            
            _button.interactable = false;
            _buttonText.text = "Loading...";

            try {
                var result = await WebUtils.SendRawDataRequestAsync(DownloadUrl);

                if (result == null) {
                    throw new Exception("Failed to download map");
                }

                await FileManager.InstallBeatmap(result, "EarthDayMap");
            } catch (Exception e) {
                Plugin.Log.Critical(e);
            }

            RefreshMap();
            _button.interactable = true;
        }

        #endregion
    }
}