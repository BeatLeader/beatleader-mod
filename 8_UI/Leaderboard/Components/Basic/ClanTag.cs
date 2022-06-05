using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ClanTag : ReeUIComponentV2 {
        #region Initialize

        protected override void OnInitialize() {
            SetMaterial();
            Clear();
            InitializeText();
        }

        #endregion

        #region Interaction

        public void Clear() {
            _container.gameObject.SetActive(false);
        }

        public void SetValue(Clan value) {
            _textComponent.text = FormatUtils.FormatClanTag(value.tag);
            _container.gameObject.SetActive(true);
            SetColor(value.color);
            SetPrefWidth();
        }

        public void SetAlpha(float alpha) {
            _alpha = alpha;
            _textComponent.alpha = alpha;
            UpdateColor();
        }

        #endregion

        #region CalculatePreferredWidth

        private const float MinWidth = 3.0f;
        private const float MaxWidth = 5.5f;
        private const float WidthPerCharacter = 1.0f;
        private float _prefWidth = MinWidth;

        public float CalculatePreferredWidth() {
            return _prefWidth;
        }

        private void SetPrefWidth() {
            var unclamped = Mathf.Min(_textComponent.preferredWidth, WidthPerCharacter * _textComponent.text.Length);
            _prefWidth = Mathf.Clamp(unclamped, MinWidth, MaxWidth);
            _containerLayoutElement.preferredWidth = _prefWidth;
        }

        #endregion

        #region TextComponent

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        private void InitializeText() {
            _textComponent.enableAutoSizing = true;
            _textComponent.fontSizeMin = 0.1f;
            _textComponent.fontSizeMax = 2.0f;
        }

        #endregion

        #region Container

        [UIComponent("container"), UsedImplicitly]
        private RectTransform _container;

        [UIComponent("container"), UsedImplicitly]
        private LayoutElement _containerLayoutElement;

        #endregion

        #region Background

        [UIComponent("background"), UsedImplicitly]
        private Image _backgroundImage;

        private float _alpha = 1.0f;
        private Color _color = Color.black;

        private void SetMaterial() {
            _backgroundImage.material = BundleLoader.ClanTagBackgroundMaterial;
        }

        private void SetColor(string strColor) {
            ColorUtility.TryParseHtmlString(strColor, out var color);
            var useDarkFont = (color.r * 0.299f + color.g * 0.687f + color.b * 0.114f) > 0.73f;
            _textComponent.color = useDarkFont ? Color.black : Color.white;
            _color = color;
            UpdateColor();
        }

        private void UpdateColor() {
            _backgroundImage.color = _color.ColorWithAlpha(_alpha);
        }

        #endregion
    }
}