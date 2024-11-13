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

        #region Events

        [UIAction("joinPressed"), UsedImplicitly]
        private void HandleJoinButtonClicked() {
            if (currentEvent == null) return;
            ReeModalSystem.OpenModal<EventDetailsDialog>(Content.transform, currentEvent);
        }

        private void OnRequestStateChanged(API.RequestState state, Paged<PlatformEvent> result, string failReason) {
            switch (state) {
                case API.RequestState.Started:
                    _previewPanel.Setup("", "Loading...", "...");
                    SetIsEmpty(false);
                    break;
                case API.RequestState.Failed:
                    _previewPanel.Setup("", "Failed to load", "");
                    SetIsEmpty(false);
                    break;
                case API.RequestState.Finished: {
                    currentEvent = result.data.FirstOrDefault();
                    var valid = currentEvent != null;
                    SetIsEmpty(!valid);
                    var date = FormatUtils.GetRelativeTimeString(currentEvent!.endDate, false);
                    _previewPanel.Setup(currentEvent.image, currentEvent.name, date);
                    break;
                }
            }
        }

        private void SetIsEmpty(bool isEmpty) {
            _emptyContainer.SetActive(isEmpty);
            _eventsContainer.SetActive(!isEmpty);
        }

        #endregion
    }
}