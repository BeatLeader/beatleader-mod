using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class AccuracyGridCell : ReeUIComponentV2 {
        #region Events

        protected override void OnInitialize() {
            InitializeBackground();
        }

        private void OnHoverStateChanged(bool isHovered) {
            _isHovered = isHovered;
            UpdateVisuals();
        }

        #endregion

        #region SetScore

        private float _score;
        private float _quality;
        private bool _isHovered;

        public void SetScore(float score, float quality) {
            _score = score;
            _quality = quality;
            UpdateVisuals();
        }

        private void UpdateVisuals() {
            if (_score <= 0) {
                _backgroundImage.color = EmptyColor;
                Text = "";
                return;
            }

            _backgroundImage.color = GetColor(_quality, _isHovered);
            Text = FormatScore(_score, _isHovered);
        }

        #endregion

        #region Formatting & Color

        private static readonly Color GoodColor = new(0f, 0.2f, 1.0f, 1.0f);
        private static readonly Color BadColor = new(0.0f, 0.1f, 0.3f, 0.1f);
        
        private static readonly Color HoverColor = new(1.0f, 0.2f, 0.5f, 0.8f);
        private static readonly Color EmptyColor = new(0.1f, 0.1f, 0.1f, 0.0f);

        private static Color GetColor(float quality, bool isHovered) {
            if (isHovered) return HoverColor;
            
            var t = quality * quality;
            return Color.Lerp(BadColor, GoodColor, t);
        }

        private static string FormatScore(float value, bool isHovered) {
            if (!isHovered) return $"{value:F1}";
            
            var acc = value / 1.15f;
            return $"<size=90%>{acc:F1}<size=60%>%";
        }

        #endregion

        #region Text

        private string _text = "";

        [UIValue("text"), UsedImplicitly]
        private string Text {
            get => _text;
            set {
                if (_text.Equals(value)) return;
                _text = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Background

        [UIComponent("background"), UsedImplicitly] private ImageView _backgroundImage;

        private void InitializeBackground() {
            _backgroundImage.material = BundleLoader.AccGridBackgroundMaterial;
            _backgroundImage.raycastTarget = true;

            var hoverController = _backgroundImage.gameObject.AddComponent<HoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
        }

        #endregion
    }
}