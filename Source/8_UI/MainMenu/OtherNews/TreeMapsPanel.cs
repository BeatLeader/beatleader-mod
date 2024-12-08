using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class TreeMapsPanel : ReeUIComponentV2 {

        public event Action<bool> ShouldDisplayOtherMapsEvent;

        [UIValue("header"), UsedImplicitly] protected NewsHeader header = null!;
        [UIObject("loading-indicator"), UsedImplicitly] private GameObject _loadingIndicator = null!;

        [UIComponent("image"), UsedImplicitly] private ImageView _image = null!;

        [UIComponent("date-text"), UsedImplicitly] private TMP_Text _dateText = null!;
        [UIComponent("top-text"), UsedImplicitly] private TMP_Text _topText = null!;
        [UIComponent("bottom-text"), UsedImplicitly] private TMP_Text _bottomText = null!;

        [UIObject("details-container"), UsedImplicitly] 
        private GameObject _detailsContainer = null!;

        private TreeStatus currentStatus;

        protected virtual void Awake() {
            header = Instantiate<NewsHeader>(transform);
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            header.Setup("Project Tree");
            _image.material = BundleLoader.RoundTexture10Material;
            _image._skew = 0.18f;
            _image.__Refresh();

            _topText.overflowMode = TextOverflowModes.Ellipsis;
            _bottomText.overflowMode = TextOverflowModes.Ellipsis;

            TreeMapRequest.SendRequest();
            TreeMapRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            TreeMapRequest.RemoveStateListener(OnRequestStateChanged);
        }

        protected async Task UpdateUI(TreeStatus status) {
            var todaysMap = status.today.song;
            _topText.SetText(todaysMap.name);
            _bottomText.SetText(todaysMap.mapper);

            _dateText.SetText($"December ${status.today.startTime.AsUnixTime().Day}th");

            if (!string.IsNullOrEmpty(todaysMap.coverImage)) {
                await _image.SetImageAsync(todaysMap.coverImage);
            }
        }

        private void OnRequestStateChanged(API.RequestState state, TreeStatus result, string failReason) {
            switch (state) {
                case API.RequestState.Uninitialized:
                case API.RequestState.Started:
                    default: {
                        _loadingIndicator.SetActive(true);
                        _detailsContainer.gameObject.SetActive(false);
                        break;
                    }
                    case API.RequestState.Failed:
                        _loadingIndicator.SetActive(false);
                        ShouldDisplayOtherMapsEvent.Invoke(true);

                        break;
                    case API.RequestState.Finished: {
                        _loadingIndicator.SetActive(false);
                        _detailsContainer.gameObject.SetActive(true);
                        if (result.today.score != null) {
                            ShouldDisplayOtherMapsEvent.Invoke(true);
                        } else {
                            currentStatus = result;
                            UpdateUI(result);
                        }
                        break;
                    }
            }
        }

        [UIAction("OnButtonPressed"), UsedImplicitly]
        void OnPlayButtonClick() {
            MapDownloadDialog.OpenSongOrDownloadDialog(currentStatus.today.song, Content.transform);
        }
    }
}
