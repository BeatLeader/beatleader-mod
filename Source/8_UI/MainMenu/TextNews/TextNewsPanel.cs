using System.Collections.Generic;
using BeatLeader.API.Methods;
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
            header.Setup("BeatLeader News");
            NewsRequest.SendRequest();
            NewsRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            NewsRequest.RemoveStateListener(OnRequestStateChanged);
        }

        #endregion

        #region Request

        private void OnRequestStateChanged(API.RequestState state, Paged<NewsPost> result, string failReason) {
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