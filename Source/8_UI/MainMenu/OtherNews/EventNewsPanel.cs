using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("header"), UsedImplicitly] private NewsHeader _header = null!;

        [UIValue("event"), UsedImplicitly] private FeaturedPreviewPanel _previewPanel = null!;

        [UIObject("empty-container"), UsedImplicitly] private GameObject _emptyContainer = null!;

        [UIObject("events-container"), UsedImplicitly] private GameObject _eventsContainer = null!;

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
                    var platformEvent = result.data.FirstOrDefault();
                    var valid = platformEvent != null;
                    SetEmptyActive(!valid);
                    if (valid) {
                        var date = FormatUtils.GetRelativeTimeString(platformEvent!.endDate, false);
                        _previewPanel.Setup(platformEvent.image, platformEvent.name, date);
                    }

                    break;
                }
            }
        }

        #endregion

        [UIAction("downloadPressed"), UsedImplicitly]
        private async void HandleDownloadButtonClicked() { }
    }
}