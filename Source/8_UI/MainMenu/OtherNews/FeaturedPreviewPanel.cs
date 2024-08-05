using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class FeaturedPreviewPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("image"), UsedImplicitly]
        private ImageView _image = null!;

        [UIComponent("image"), UsedImplicitly]
        private RectTransform _imageRect = null!;

        [UIObject("image-container"), UsedImplicitly]
        private GameObject _imageContainer = null!;

        [UIObject("container"), UsedImplicitly]
        private GameObject _container = null!;

        [UIComponent("top-text"), UsedImplicitly]
        private TMP_Text _topText = null!;

        [UIComponent("top-text"), UsedImplicitly]
        private RectTransform _topTextRect = null!;

        [UIComponent("bottom-text"), UsedImplicitly]
        private RectTransform _bottomTextRect = null!;

        [UIComponent("bottom-text"), UsedImplicitly]
        private TMP_Text _bottomText = null!;

        #endregion

        #region Layout

        // the basic one did not work well, so I made a custom one using LayoutElement
        private class AspectFitter : MonoBehaviour {
            private RectTransform _rectTransform = null!;
            private LayoutElement _layoutElement = null!;

            public void Setup(LayoutElement element) {
                _layoutElement = element;
            }

            private void Awake() {
                _rectTransform = GetComponent<RectTransform>();
            }

            private void OnRectTransformDimensionsChange() {
                _layoutElement.preferredWidth = _rectTransform.rect.height;
            }
        }

        #endregion

        #region Setup

        public async void Setup(string previewUrl, string topText, string bottomText) {
            _topText.text = $" {topText}";
            _bottomText.text = bottomText;
            if (!string.IsNullOrEmpty(previewUrl)) {
                await _image.SetImageAsync(previewUrl);
            }
        }

        protected override void OnInitialize() {
            var element = _imageContainer.AddComponent<LayoutElement>();
            var fitter = _container.AddComponent<AspectFitter>();
            fitter.Setup(element);
            //
            _image.GetComponent<LayoutElement>().TryDestroy();
            _imageRect.anchorMin = Vector2.zero;
            _imageRect.anchorMax = Vector2.one;
            _imageRect.sizeDelta = Vector2.zero;
            //
            _image.material = BundleLoader.RoundTexture10Material;
            _image.preserveAspect = true;
            _image._skew = 0.18f;
            _image.__Refresh();
        }

        #endregion
    }
}