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
        }

        #endregion

        #region Interaction

        public void Clear() {
            _container.gameObject.SetActive(false);
        }

        public void SetValue(Clan value) {
            SetColor(value.color);
            textComponent.text = FormatUtils.FormatClanTag(value.tag);
            _container.gameObject.SetActive(true);
        }

        public void SetAlpha(float alpha) {
            _alpha = alpha;
            textComponent.alpha = alpha;
            UpdateColor();
        }

        #endregion

        #region CalculatePreferredWidth

        private const float MinWidth = 3.4f;

        public float CalculatePreferredWidth() {
            return Mathf.Max(MinWidth, textComponent.preferredWidth);
        }

        #endregion

        #region TextComponent

        [UIComponent("text-component"), UsedImplicitly]
        public TextMeshProUGUI textComponent;

        #endregion

        #region Container
        
        [UIComponent("container"), UsedImplicitly]
        private RectTransform _container;

        #endregion

        #region Background

        [UIComponent("background"), UsedImplicitly]
        private Image _backgroundImage;

        private float _alpha = 1.0f;
        private Color _color = Color.black;

        private void SetMaterial() {
            _backgroundImage.material = BundleLoader.ClanTagBackgroundMaterial;
        }

        private void SetColor(string colorString) {
            if (!ColorUtility.TryParseHtmlString(colorString, out _color)) _color = Color.white;
            UpdateColor();
        }

        private void UpdateColor() {
            _backgroundImage.color = _color.ColorWithAlpha(_alpha);
        }

        #endregion
    }
}