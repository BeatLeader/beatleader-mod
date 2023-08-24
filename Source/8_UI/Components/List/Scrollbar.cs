using System;
using System.Linq;
using HMUI;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    /// <summary>
    /// Abstraction for list's scrollbar
    /// </summary>
    internal interface IScrollbar {
        float PageHeight { set; }
        float Progress { set; }
        
        bool CanScrollUp { set; }
        bool CanScrollDown { set; }
        
        event Action<ScrollView.ScrollDirection>? ScrollEvent;
    }

    /// <summary>
    /// Scrollbar for ReeUIComponentV3 lists
    /// </summary>
    internal class Scrollbar : LayoutComponentBase<Scrollbar>, IScrollbar {
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

        #endregion
        
        #region Setup

        private static Transform? _prefab;

        private RectTransform _scrollbarRect = null!;
        private VerticalScrollIndicator _scrollIndicator = null!;
        private Button _upButton = null!;
        private Button _downButton = null!;

        protected override void OnInitialize() {
            if (!_prefab) {
                _prefab = Resources.FindObjectsOfTypeAll<VerticalScrollIndicator>()
                    .FirstOrDefault(x => x.gameObject.activeSelf)?.transform.parent;
            }
            var instance = Instantiate(_prefab, ContentTransform);
            _scrollbarRect = (RectTransform)instance!.transform;
            _scrollIndicator = instance.GetComponentInChildren<VerticalScrollIndicator>();
            _scrollIndicator.GetComponent<Image>().enabled = true;

            var emptyNavigation = new Navigation { mode = Navigation.Mode.None };
            var buttons = instance.GetComponentsInChildren<Button>();

            _upButton = buttons.First(x => x.name == "UpButton");
            _upButton.navigation = emptyNavigation;
            _upButton.onClick.AddListener(HandleUpButtonClicked);
            EnableAllComponents(_upButton.gameObject);

            _downButton = buttons.First(x => x.name == "DownButton");
            _downButton.navigation = emptyNavigation;
            _downButton.onClick.AddListener(HandleDownButtonClicked);
            EnableAllComponents(_downButton.gameObject);
        }

        protected override void OnLayoutPropertySet() {
            _scrollbarRect.sizeDelta = new Vector2(Width, Height);
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