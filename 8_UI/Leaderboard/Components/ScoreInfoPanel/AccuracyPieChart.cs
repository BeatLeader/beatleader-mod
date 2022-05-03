using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class AccuracyPieChart : ReeUIComponentV2 {
        #region Events

        protected override void OnInitialize() {
            SetMaterials();
        }

        #endregion

        #region Type

        public enum Type {
            Left,
            Right
        }

        #endregion

        #region SetValues

        public void SetValues(Type type, float score) {
            _backgroundImage.color = type == Type.Left ? LeftColor : RightColor;
            Text = FormatScore(score);
            SetFillValue(CalculateFillValue(score));
        }

        #endregion

        #region Formatting

        private static readonly Color LeftColor = new(0.8f, 0.2f, 0.2f, 0.1f);
        private static readonly Color RightColor = new(0.2f, 0.2f, 0.8f, 0.1f);

        private static float CalculateFillValue(float score) {
            var ratio = Mathf.Clamp01((score - 65) / 50.0f);
            return Mathf.Pow(ratio, 0.6f);
        }

        private static string FormatScore(float value) {
            return $"{value:F2}";
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

        private void SetMaterials() {
            _materialInstance = Material.Instantiate(BundleLoader.HandAccIndicatorMaterial);
            _backgroundImage.material = _materialInstance;
        }

        #endregion
    }
}