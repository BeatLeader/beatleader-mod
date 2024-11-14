using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class AbstractNewsPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("header"), UsedImplicitly] protected NewsHeader header = null!;

        [UIComponent("scroll-view"), UsedImplicitly] protected ScrollView scrollView = null!;

        [UIComponent("main-container"), UsedImplicitly] protected Transform mainContainer = null!;

        [UIComponent("height-wrapper"), UsedImplicitly] protected HorizontalLayoutGroup heightWrapper = null!;

        [UIComponent("background"), UsedImplicitly] protected ImageView background = null!;

        protected virtual void Awake() {
            header = Instantiate<NewsHeader>(transform);
        }

        protected override void OnInitialize() {
            background.raycastTarget = true;
            InitializeScrollView(scrollView);
        }

        protected virtual void Update() {
            UpdateScrollbarIfDirty();
        }

        #endregion

        #region InitializeScrollView

        private bool _scrollbarDirty = false;
        private int _skipFrames;

        protected void MarkScrollbarDirty() {
            _scrollbarDirty = true;
            _skipFrames = 2;
        }

        private void UpdateScrollbarIfDirty() {
            if (!_scrollbarDirty || --_skipFrames > 0) return;
            _scrollbarDirty = false;
            scrollView.SetContentSize(heightWrapper.preferredHeight);
            scrollView.ScrollTo(0, false);
        }

        #endregion

        #region InitializeScrollView

        private static void InitializeScrollView(ScrollView scrollView) {
            scrollView._pageStepNormalizedSize = 0.5f;

            var scrollBar = (RectTransform)scrollView.transform.GetChild(0);
            var upIcon = (RectTransform)scrollBar.GetChild(0).GetChild(0);
            var downIcon = (RectTransform)scrollBar.GetChild(1).GetChild(0);

            scrollBar.sizeDelta = new Vector2(4, 0);
            upIcon.anchoredPosition = new Vector2(-2, -4);
            downIcon.anchoredPosition = new Vector2(-2, 4);
            scrollBar.ForceUpdateRectTransforms();

            var viewport = (RectTransform)scrollView.transform.GetChild(1);
            viewport.offsetMin = new Vector2(0, 0);
            viewport.offsetMax = new Vector2(0, 0);
        }

        #endregion
    }
}