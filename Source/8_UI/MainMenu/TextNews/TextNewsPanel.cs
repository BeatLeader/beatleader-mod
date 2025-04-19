using System.Collections.Generic;
using BeatLeader.API;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPanel : AbstractNewsPanel {
        #region Components

        [UIComponent("empty-text"), UsedImplicitly] private TextMeshProUGUI _emptyText = null!;

        [UIObject("loading-indicator"), UsedImplicitly] private GameObject _loadingIndicator = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            NewsRequest.SendRequest();
            NewsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDispose() {
            NewsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        #endregion

        #region Request

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<NewsPost>> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Uninitialized:
                case WebRequests.RequestState.Started:
                default: {
                    _loadingIndicator.SetActive(true);
                    _emptyText.gameObject.SetActive(false);
                    DisposeList();
                    break;
                }
                case WebRequests.RequestState.Failed:
                    _loadingIndicator.SetActive(false);
                    _emptyText.gameObject.SetActive(true);
                    _emptyText.text = "<color=#ff8888>Failed to load";
                    DisposeList();
                    break;
                case WebRequests.RequestState.Finished: {
                    _loadingIndicator.SetActive(false);

                    if (instance.Result.data is { Count: > 0 }) {
                        _emptyText.gameObject.SetActive(false);
                        PresentList(instance.Result.data);
                    } else {
                        _emptyText.gameObject.SetActive(true);
                        _emptyText.text = "There is no news";
                        DisposeList();
                    }

                    break;
                }
            }
        }

        #endregion

        #region List

        private readonly List<TextNewsPostPanel> _list = new List<TextNewsPostPanel>();

        private void PresentList(IEnumerable<NewsPost> items) {
            DisposeList();

            foreach (var item in items) {
                var component = Instantiate<TextNewsPostPanel>(transform);
                component.ManualInit(mainContainer);
                component.Setup(item);
                _list.Add(component);
            }

            MarkScrollbarDirty();
        }

        private void DisposeList() {
            foreach (var component in _list) {
                Destroy(component.gameObject);
            }

            _list.Clear();
            MarkScrollbarDirty();
        }

        #endregion
    }
}