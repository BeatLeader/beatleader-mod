using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using HMUI;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("header"), UsedImplicitly] private NewsHeader _header = null!;

        [UIValue("event"), UsedImplicitly] private FeaturedPreviewPanel _previewPanel = null!;

        [UIObject("empty-container"), UsedImplicitly] private GameObject _emptyContainer = null!;

        [UIObject("events-container"), UsedImplicitly] private GameObject _eventsContainer = null!;

        [UIComponent("loading-modal"), UsedImplicitly]
        private ModalView _modal = null!;

        [UIObject("finished-container"), UsedImplicitly]
        private GameObject _finishedContainer = null!;

        [UIComponent("finished-text"), UsedImplicitly]
        private TMP_Text _finishedText = null!;

        [UIObject("loading-container"), UsedImplicitly]
        private GameObject _loadingContainer = null!;

        PlatformEvent? currentEvent = null;

        private void Awake() {
            _header = Instantiate<NewsHeader>(transform);
            _previewPanel = Instantiate<FeaturedPreviewPanel>(transform);
        }

        #endregion

        #region Setup

        public void Reload() {
            PlatformEventsRequest.SendRequest();
        }

        protected override void OnInitialize() {
            _header.Setup("Events");
            PlatformEventsRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            PlatformEventsRequest.RemoveStateListener(OnRequestStateChanged);
        }

        #endregion

        #region Request

        private void SetEmptyActive(bool active) {
            _emptyContainer.SetActive(active);
            _eventsContainer.SetActive(!active);
        }

        private void OnRequestStateChanged(API.RequestState state, Paged<PlatformEvent> result, string failReason) {
            switch (state) {
                case API.RequestState.Started:
                    _previewPanel.Setup("", "Loading...", "...");
                    SetEmptyActive(false);
                    break;
                case API.RequestState.Failed:
                    _previewPanel.Setup("", "Failed to load", "");
                    SetEmptyActive(false);
                    break;
                case API.RequestState.Finished: {
                    currentEvent = result.data.FirstOrDefault();
                    var valid = currentEvent != null;
                    SetEmptyActive(!valid);
                    if (valid) {
                        if (currentEvent.description != null) {
                            _finishedText.SetText(currentEvent.description);
                        }
                        var date = FormatUtils.GetRelativeTimeString(currentEvent!.endDate, false);
                        _previewPanel.Setup(currentEvent.image, currentEvent.name, date);
                        DownloadButtonActive = currentEvent.downloadable;
                    }

                    break;
                }
            }
        }

        #endregion

        private bool _downloadButtonActive;

        [UIValue("download-button-active"), UsedImplicitly]
        private bool DownloadButtonActive {
            get => _downloadButtonActive;
            set {
                if (_downloadButtonActive.Equals(value)) return;
                _downloadButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("download-button-click"), UsedImplicitly]
        private void HandleDownloadButtonClicked() {
            _finishedContainer.SetActive(false);
            _loadingContainer.SetActive(true);
            void OnSuccess(byte[] bytes) {
                var filename = currentEvent.name.Replace(" ", "_");
                FileManager.DeletePlaylist(filename);

                if (FileManager.TrySaveRankedPlaylist(filename, bytes)) {
                    PlaylistsLibInterop.TryRefreshPlaylists(true);
                    SongCore.Loader.Instance.RefreshSongs(false);
                }
            }

            void OnFail(string reason) {
                Plugin.Log.Debug($"Event {currentEvent.name} playlist update failed: {reason}");
            }

            StartCoroutine(PlaylistRequest.SendRequest(currentEvent.playlistId.ToString(), OnSuccess, OnFail));

            _modal.Hide(true);
        }

        [UIAction("joinPressed"), UsedImplicitly]
        private async void HandleJoinButtonClicked() { 
            _finishedContainer.SetActive(true);
            _loadingContainer.SetActive(false);
            _modal.Show(true, true);
        }
    }
}