using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class SettingsContainer : ReeUIComponentV2 {
        #region UI Sizing
        //i really hate this way, but unity dont calculate disabled layout objects
        [UIValue("height")] private const int Height = 46;
        [UIValue("width")] private const int Width = 55;
        [UIValue("scrollbar-width")] private const int ScrollbarWidth = 4;
        [UIValue("padding")] private const int Padding = 1;

        [UIValue("bg-width")] private int _BGWidth => Width - ScrollbarWidth;
        [UIValue("scrollable-width")] private int _ScrollableWidth => Width - (Padding * 2);

        #endregion

        #region UI Components

        [UIComponent("content-view-scrollable")] private readonly BSMLScrollableContainer _scrollView;
        [UIComponent("scrollbar-container")] private readonly Transform _scrollbarContainer;
        [UIComponent("container-group")] private readonly RectTransform _containerGroup;
        [UIComponent("content-group")] private readonly RectTransform _contentGroup;
        [UIComponent("content-host")] private readonly Transform _contentHost;

        [UIObject("navigation-button-container")] 
        private readonly GameObject _navigationButtonContainer;

        [UIValue("navigation-button")] 
        private NavigationButton _navigationButton;

        #endregion

        #region Setup

        private ContentView _hostContentView;

        public void Setup(ContentView contentView) {
            if (_hostContentView != null) {
                _hostContentView.ContentWasPresentedEvent -= HandleContentWasPresented;
                _hostContentView.ContentWasDismissedEvent -= HandleContentWasDismissed;
                _hostContentView.SetViewParent(null);
            }
            _hostContentView = contentView;
            _hostContentView.ContentWasPresentedEvent += HandleContentWasPresented;
            _hostContentView.ContentWasDismissedEvent += HandleContentWasDismissed;
            _hostContentView.SetViewParent(_contentHost);
        }

        protected override void OnInstantiate() {
            _navigationButton = Instantiate<NavigationButton>(transform);
        }
        protected override void OnInitialize() {
            SetupScrollbar();
            RefreshContent();

            _navigationButton.OnClick += HandleNavigationButtonClicked;
            _navigationButton.Flow = NavigationButton.FlowDirection.Backward;
            _navigationButton.Text = "Back";
            _navigationButton.Color = Color.red;
        }

        private void SetupScrollbar() {
            var original = Resources.FindObjectsOfTypeAll<VerticalScrollIndicator>()
                .FirstOrDefault(x => x.gameObject.activeSelf)?.transform.parent;
            if (original == null) return;
            var instance = Instantiate(original, _scrollbarContainer, false);
            var scrollbar = instance.GetComponentInChildren<VerticalScrollIndicator>();

            var emptyNavigation = new Navigation() { mode = Navigation.Mode.None };
            var buttons = instance.GetComponentsInChildren<Button>();

            var upButton = buttons.FirstOrDefault(x => x.name == "UpButton");
            upButton.navigation = emptyNavigation;
            EnableAllComponents(upButton.gameObject);

            var downButton = buttons.FirstOrDefault(x => x.name == "DownButton");
            downButton.navigation = emptyNavigation;
            EnableAllComponents(downButton.gameObject);

            _scrollView.PageUpButton = upButton;
            _scrollView.PageDownButton = downButton;
            _scrollView.ScrollIndicator = scrollbar;

            instance.localPosition = new(2, 0);
        }

        private void EnableAllComponents(GameObject go) {
            foreach (var item in go.GetComponents<Behaviour>()) {
                item.enabled = true;
            }
        }

        #endregion

        #region UI Callbacks

        private void HandleContentWasPresented(ContentView view, ContentView presented) {
            _navigationButtonContainer.SetActive(true);
            RunNavigationRebuild();
            _viewsHierarchy.Push(view);
            RefreshContent();
        }
        private void HandleContentWasDismissed(ContentView view, ContentView presented) {
            _viewsHierarchy.Pop();
        }

        #endregion

        #region Navigation

        private Stack<ContentView> _viewsHierarchy = new();

        private void HandleNavigationButtonClicked() {
            if (!_viewsHierarchy.TryPeek(out var view)) return;

            view.DismissView();
            _navigationButtonContainer.SetActive(_viewsHierarchy.Count != 0);
            RefreshContent();
        }

        public void NotifyModalHidden() {
            while (_viewsHierarchy.Count > 0)
                HandleNavigationButtonClicked();
            RefreshContent();
        }

        public void RefreshContent() {
            RunContentRebuild();
            _scrollView.ContentSizeUpdated();
        }

        #endregion

        #region Rebuild

        private bool _isBuilt;

        private void RunNavigationRebuild() {
            if (_isBuilt) return;

            var coroutine = BasicCoroutines.RebuildUICoroutine(_containerGroup);
            CoroutinesHandler.instance.StartCoroutine(coroutine, () => _isBuilt = true);
        }

        private void RunContentRebuild() {
            var coroutine = BasicCoroutines.RebuildUICoroutine(_contentGroup);
            CoroutinesHandler.instance.StartCoroutine(coroutine);
        }

        #endregion
    }
}
