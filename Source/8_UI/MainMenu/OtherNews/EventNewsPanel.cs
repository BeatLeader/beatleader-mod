using System;
using System.Collections.Generic;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : AbstractNewsPanel {
        #region UI Components

        [UIComponent("empty-text"), UsedImplicitly] private TextMeshProUGUI _emptyText = null!;

        [UIObject("loading-indicator"), UsedImplicitly] private GameObject _loadingIndicator = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            header.Setup("BeatLeader Events");
            PlatformEventsRequest.SendRequest();
            PlatformEventsRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            PlatformEventsRequest.RemoveStateListener(OnRequestStateChanged);
        }

        #endregion

        #region Events

        private void OnRequestStateChanged(API.RequestState state, Paged<PlatformEvent> result, string failReason) {
            switch (state) {
                case API.RequestState.Uninitialized:
                case API.RequestState.Started:
                default: {
                    _loadingIndicator.SetActive(true);
                    _emptyText.gameObject.SetActive(false);
                    DisposeList();
                    break;
                }
                case API.RequestState.Failed:
                    _loadingIndicator.SetActive(false);
                    _emptyText.gameObject.SetActive(true);
                    _emptyText.text = "<color=#ff8888>Failed to load";
                    DisposeList();
                    break;
                case API.RequestState.Finished: {
                    _loadingIndicator.SetActive(false);

                    if (result.data is { Count: > 0 }) {
                        _emptyText.gameObject.SetActive(false);
                        PresentList(result.data);
                    } else {
                        _emptyText.gameObject.SetActive(true);
                        _emptyText.text = "There is no events";
                        DisposeList();
                    }

                    break;
                }
            }
        }

        #endregion

        #region List

        private readonly List<FeaturedPreviewPanel> _list = new List<FeaturedPreviewPanel>();

        private void PresentList(IEnumerable<PlatformEvent> items) {
            DisposeList();

            foreach (var item in items) {
                var component = Instantiate<FeaturedPreviewPanel>(transform);
                component.ManualInit(mainContainer);
                SetupFeaturePreview(component, item);
                _list.Add(component);
            }

            MarkScrollbarDirty();
        }

        private void SetupFeaturePreview(FeaturedPreviewPanel panel, PlatformEvent item) {
            string bottomText;
            var timeSpan = FormatUtils.GetRelativeTime(item.endDate);
            if (timeSpan < TimeSpan.Zero) {
                bottomText = "<color=#88FF88>Ongoing!";
            } else {
                var date = FormatUtils.GetRelativeTimeString(timeSpan, false);
                bottomText = $"<color=#884444>Ended {date}";
            }

            panel.Setup(item.image, item.name, bottomText, "Details", ButtonAction);
            return;

            void ButtonAction() {
                ReeModalSystem.OpenModal<EventDetailsDialog>(Content.transform, item);
            }
        }


        private void DisposeList() {
            foreach (var post in _list) {
                Destroy(post.gameObject);
            }

            _list.Clear();
            MarkScrollbarDirty();
        }

        #endregion
    }
}