using System;
using System.Linq;
using HMUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UButton = UnityEngine.UI.Button;
using UImage = UnityEngine.UI.Image;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for list's scrollbar
    /// </summary>
    internal interface IScrollbar {
        float PageHeight { set; }
        float Progress { set; }

        bool CanScrollUp { set; }
        bool CanScrollDown { set; }

        event Action<ScrollView.ScrollDirection>? ScrollEvent;

        void SetActive(bool active);
    }

    /// <summary>
    /// Scrollbar for ReactiveComponent lists
    /// </summary>
    internal class Scrollbar : ReactiveComponent, IScrollbar {
        #region Events

        public event Action<ScrollView.ScrollDirection>? ScrollEvent;

        #endregion

        #region Impl

        float IScrollbar.PageHeight {
            set => _scrollIndicator.normalizedPageHeight = value;
        }

        float IScrollbar.Progress {
            set => _scrollIndicator.progress = value;
        }

        bool IScrollbar.CanScrollUp {
            set => _upButton.interactable = value;
        }

        bool IScrollbar.CanScrollDown {
            set => _downButton.interactable = value;
        }

        public void SetActive(bool active) {
            Content!.SetActive(active);
        }

        #endregion

        #region Setup

        private static Transform? _prefab;

        private RectTransform _scrollbarRect = null!;
        private VerticalScrollIndicator _scrollIndicator = null!;
        private UButton _upButton = null!;
        private UButton _downButton = null!;

        protected override void OnInitialize() {
            if (!_prefab) {
                _prefab = Resources.FindObjectsOfTypeAll<VerticalScrollIndicator>()
                    .FirstOrDefault(x => x.gameObject.activeSelf)?.transform.parent;
            }
            
            var instance = Object.Instantiate(_prefab, ContentTransform);
            _scrollbarRect = (RectTransform)instance!;
            _scrollbarRect.anchorMin = Vector2.zero;
            _scrollbarRect.anchorMax = Vector2.one;
            _scrollbarRect.sizeDelta = Vector2.zero;
            _scrollIndicator = instance!.GetComponentInChildren<VerticalScrollIndicator>();
            _scrollIndicator.GetComponent<UImage>().enabled = true;

            var buttons = instance.GetComponentsInChildren<UButton>();
            _upButton = buttons.First(x => x.name == "UpButton");
            HandleButton(_upButton, HandleUpButtonClicked);
            _downButton = buttons.First(x => x.name == "DownButton");
            HandleButton(_downButton, HandleDownButtonClicked);

            static void HandleButton(UButton button, UnityAction action) {
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(action);
                EnableAllComponents(button.gameObject);
                var rect = (RectTransform)button.transform;
                rect.anchorMin = new(0.5f, 0);
                
                var anchorMax = rect.anchorMax;
                rect.anchorMax = new(0.5f, anchorMax.y);
                
                var pos = rect.localPosition;
                rect.localPosition = new(2f, pos.y);
                
                var size = rect.sizeDelta;
                rect.sizeDelta = new(4f, size.y);
            }
        }
        
        private static void EnableAllComponents(GameObject go) {
            foreach (var item in go.GetComponents<Behaviour>()) {
                item.enabled = true;
            }
        }

        #endregion

        #region Callbacks

        private void HandleUpButtonClicked() {
            ScrollEvent?.Invoke(ScrollView.ScrollDirection.Up);
        }

        private void HandleDownButtonClicked() {
            ScrollEvent?.Invoke(ScrollView.ScrollDirection.Down);
        }

        #endregion
    }
}