using System;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class AccuracyPieChart : ReeUIComponentV2 {
        #region Events

        protected override void OnInitialize() {
            InitializeBackground();
        }

        private void OnHoverStateChanged(bool isHovered) {
            _isHovered = isHovered;
            UpdateVisuals();
        }

        #endregion

        #region Type

        public enum Type {
            Left,
            Right
        }

        #endregion

        #region SetValues

        private Type _type;
        private float _score;
        private bool _isHovered;

        public void SetValues(Type type, float score) {
            _type = type;
            _score = score;
            UpdateVisuals();
            SetFillValue(CalculateFillValue(score));
        }

        private void UpdateVisuals() {
            Text = FormatScore(_score, _isHovered);
            _backgroundImage.color = GetColor(_type, _isHovered);
        }

        #endregion

        #region Formatting

        private static readonly Color LeftColor = new(0.8f, 0.2f, 0.2f, 0.1f);
        private static readonly Color RightColor = new(0.2f, 0.2f, 0.8f, 0.1f);
        private const float HoveredGlow = 0.5f;

        private static Color GetColor(Type type, bool isHovered) {
            var col = type switch {
                Type.Left => LeftColor,
                Type.Right => RightColor,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            if (isHovered) col.a = HoveredGlow;
            return col;
        }

        private static float CalculateFillValue(float score) {
            var ratio = Mathf.Clamp01((score - 65) / 50.0f);
            return Mathf.Pow(ratio, 0.6f);
        }
        
        private static string FormatScore(float value, bool isHovered) {
            if (!isHovered) return $"{value:F2}";
            
            var acc = value / 1.15f;
            return $"{acc:F2}<size=60%>%";
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

        [UIComponent("background"), UsedImplicitly]
        private ImageView _backgroundImage;

        private static readonly int FillPropertyId = Shader.PropertyToID("_FillValue");
        private Material _materialInstance;

        private void SetFillValue(float value) {
            _materialInstance.SetFloat(FillPropertyId, value);
        }

        private void InitializeBackground() {
            _materialInstance = Material.Instantiate(BundleLoader.HandAccIndicatorMaterial);
            _backgroundImage.material = _materialInstance;
            _backgroundImage.raycastTarget = true;
            var hoverController = _backgroundImage.gameObject.AddComponent<HoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
        }

        #endregion
    }
}