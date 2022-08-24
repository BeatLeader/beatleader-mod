using System;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class AccuracyPieChart : ReeUIComponentV2 {
        #region Events

        protected override void OnInitialize() {
            InitializeBackground();
        }

        private void OnHoverStateChanged(bool isHovered, float progress) {
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

        public void SetValues(Type type, float score) {
            _type = type;
            _score = score;
            UpdateVisuals();
            SetFillValue(CalculateFillValue(score));
        }

        private void UpdateVisuals() {
            _backgroundImage.color = GetColor(_type, _hoverController.Progress);
            _textComponent.text = FormatScore(_score, _hoverController.IsHovered);
        }

        #endregion

        #region Formatting

        private static readonly Color LeftColor = new(0.8f, 0.2f, 0.2f, 0.1f);
        private static readonly Color RightColor = new(0.2f, 0.2f, 0.8f, 0.1f);
        private const float HoveredGlow = 0.5f;

        private static Color GetColor(Type type, float hover) {
            var col = type switch {
                Type.Left => LeftColor,
                Type.Right => RightColor,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            col.a = Mathf.Lerp(col.a, HoveredGlow, hover);
            return col;
        }

        private static float CalculateFillValue(float score) {
            var ratio = Mathf.Clamp01((score - 65) / 50.0f);
            return Mathf.Pow(ratio, 0.6f);
        }
        
        private static string FormatScore(float value, bool showAcc) {
            if (!showAcc) return $"{value:F2}";
            var acc = value / 1.15f;
            return $"<line-height=53%>{value:F2}\n<size=80%>{acc:F2}<size=50%>%";
        }

        #endregion

        #region Text

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        #endregion

        #region Background

        [UIComponent("background"), UsedImplicitly]
        private ImageView _backgroundImage;

        private SmoothHoverController _hoverController;

        private static readonly int FillPropertyId = Shader.PropertyToID("_FillValue");
        private Material _materialInstance;

        private void SetFillValue(float value) {
            _materialInstance.SetFloat(FillPropertyId, value);
        }

        private void InitializeBackground() {
            _materialInstance = Material.Instantiate(BundleLoader.HandAccIndicatorMaterial);
            _backgroundImage.material = _materialInstance;
            _backgroundImage.raycastTarget = true;
            _hoverController = _backgroundImage.gameObject.AddComponent<SmoothHoverController>();
            _hoverController.HoverStateChangedEvent += OnHoverStateChanged;
        }

        #endregion
    }
}