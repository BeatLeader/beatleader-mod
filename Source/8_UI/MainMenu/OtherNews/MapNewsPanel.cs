using System.Collections.Generic;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class MapNewsPanel : AbstractNewsPanel {
        #region UI Components

        [UIComponent("empty-text"), UsedImplicitly] private TextMeshProUGUI _emptyText = null!;

        [UIObject("loading-indicator"), UsedImplicitly] private GameObject _loadingIndicator = null!;

        protected virtual void Awake() {
            header = Instantiate<NewsHeader>(transform);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            header.Setup("Trending Maps");
            TrendingMapsRequest.SendRequest();
            TrendingMapsRequest.AddStateListener(OnRequestStateChanged);
        }

        protected override void OnDispose() {
            TrendingMapsRequest.RemoveStateListener(OnRequestStateChanged);
        }

        #endregion

        #region Request

        private void OnRequestStateChanged(API.RequestState state, Paged<TrendingMapData> result, string failReason) {
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
                        _emptyText.text = "There is no trending maps";
                        DisposeList();
                    }

                    break;
                }
            }
        }

        #endregion

        #region List

        private readonly List<FeaturedPreviewPanel> _list = new List<FeaturedPreviewPanel>();

        private void PresentList(IEnumerable<TrendingMapData> items) {
            DisposeList();

            foreach (var item in items.Where(m => m.difficulty?.status != 5)) {
                var component = Instantiate<FeaturedPreviewPanel>(transform);
                component.ManualInit(mainContainer);
                SetupFeaturePreview(component, item);
                _list.Add(component);
            }

            MarkScrollbarDirty();
        }

        private void SetupFeaturePreview(FeaturedPreviewPanel panel, TrendingMapData item) {
            panel.Setup(item.song.coverImage, item.song.name, item.song.mapper, "Play", ButtonAction, BackgroundAction);
            return;

            void ButtonAction() {
                MapDownloadDialog.OpenSongOrDownloadDialog(item.song, Content.transform);
            }

            void BackgroundAction() {
                MapPreviewDialog.OpenSongOrDownloadDialog(item, Content.transform);
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