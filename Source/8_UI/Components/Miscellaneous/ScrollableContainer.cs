using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ScrollableContainer : LayoutComponentBase<ScrollableContainer> {
        #region ManualInitScrollView

        [HarmonyPatch(typeof(ScrollView), "Awake")]
        private class ManualInitScrollView : ScrollView {
            public bool preventAwakeInvocation = true;

            [UsedImplicitly]
            private static bool Prefix(object __instance) {
                return __instance is not ManualInitScrollView { preventAwakeInvocation: true };
            }
        }

        #endregion

        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public IScrollbar? Scrollbar {
            get => _scrollbar;
            set {
                if (_scrollbar is not null) _scrollbar.ScrollEvent -= HandleScrollbarScroll;
                _scrollbar = value;
                if (_scrollbar is not null) _scrollbar.ScrollEvent += HandleScrollbarScroll;
                _scrollView.enabled = true;
            }
        }

        private IScrollbar? _scrollbar;

        #endregion

        #region Scrollbar

        private void UpdateScrollbar(float pos) {
            if (_scrollbar is null) return;
            //TODO: asm pub; pub ver:
            //var contentRect = _scrollView._contentRectTransform.rect;
            //var contentRect = _scrollView._contentRectTransform.rect;
            //var viewportRect = _scrollView._viewport.rect;
            //_scrollbar.PageHeight = ScrollDirection is ScrollView.ScrollViewDirection.Vertical ?
            //    viewportRect.height / contentRect.height :
            //    viewportRect.width / contentRect.width;
            //var progress = pos / (_scrollView.contentSize - _scrollView.scrollPageSize);
            //_scrollbar.Progress = progress;
            //_scrollbar.CanScrollUp = _scrollView._destinationPos > 1f / 1000;
            //_scrollbar.CanScrollDown = _scrollView._destinationPos < _scrollView.contentSize - _scrollView.scrollPageSize - 1f / 1000;

            var contentRect = _scrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").rect;
            var viewportRect = _scrollView.GetField<RectTransform, ScrollView>("_viewport").rect;
            _scrollbar.PageHeight = ScrollDirection is ScrollView.ScrollViewDirection.Vertical ?
                viewportRect.height / contentRect.height :
                viewportRect.width / contentRect.width;
            var progress = pos / (_scrollView.GetProperty<float, ScrollView>("contentSize") -
                _scrollView.GetProperty<float, ScrollView>("scrollPageSize"));
            _scrollbar.Progress = progress;
            _scrollbar.CanScrollUp = _scrollView.GetField<float, ScrollView>("_destinationPos") > 1f / 1000;
            _scrollbar.CanScrollDown = _scrollView.GetField<float, ScrollView>("_destinationPos")
                < _scrollView.GetProperty<float, ScrollView>("contentSize")
                - _scrollView.GetProperty<float, ScrollView>("scrollPageSize") - 1f / 1000;
        }

        private void HandleScrollbarScroll(ScrollView.ScrollDirection scrollDirection) {
            switch (scrollDirection) {
                case ScrollView.ScrollDirection.Up:
                    _scrollView.PageUpButtonPressed();
                    break;
                case ScrollView.ScrollDirection.Down:
                    _scrollView.PageDownButtonPressed();
                    break;
            }
        }

        #endregion

        #region Construct

        //TODO: asm pub
        public ScrollView.ScrollViewDirection ScrollDirection {
            get => _scrollView.GetField<ScrollView.ScrollViewDirection, ScrollView>("_scrollViewDirection");
            set => _scrollView.SetField("_scrollViewDirection", value);
        }

        private ScrollView _scrollView = null!;
        private ContentSizeFitter _contentFitter = null!;

        protected override void OnPropertySet() {
            _contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        protected override void OnConstruct(Transform parent) {
            var containerRect = CreateObject(parent, "Container");
            var containerGo = containerRect.gameObject;
            containerGo.AddComponent<Touchable>();
            containerGo.AddComponent<EventSystemListener>();

            var viewportRect = CreateObject(containerRect, "Viewport");
            var viewportGo = viewportRect.gameObject;
            viewportGo.AddComponent<RectMask2D>();

            var contentRect = CreateObject(viewportRect, "Content");
            var contentGo = contentRect.gameObject;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);

            _contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            _contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            _contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var verticalLayoutGroup = contentGo.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childForceExpandWidth = false;

            //TODO: asm pub
            var scrollView = containerGo.AddComponent<ManualInitScrollView>();
            _scrollView = scrollView;
            _scrollView.SetField("_viewport", viewportRect);
            _scrollView.SetField("_contentRectTransform", contentRect);
            _scrollView.SetField("_platformHelper", BeatSaberUI.PlatformHelper);
            _scrollView.scrollPositionChangedEvent += UpdateScrollbar;
            scrollView.preventAwakeInvocation = false;
            _scrollView.Awake();
            _scrollView.enabled = true;

            ApplyBSMLRoot(contentGo);

            static RectTransform CreateObject(Transform parent, string name) {
                var go = new GameObject(name);
                var rect = go.AddComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                return rect;
            }
        }

        #endregion
    }
}