using System;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyDetailsRow.bsml")]
    internal class AccuracyDetailsRow : ReeUIComponent {
        #region Events

        protected override void OnInitialize() {
            SetMaterials();
        }

        #endregion

        #region Type

        public enum Type {
            TD,
            Pre,
            Post
        }

        #endregion

        #region SetValues

        public void SetValues(Type type, float leftValue, float rightValue) {
            Label = type.ToString();
            LeftValue = Format(type, leftValue);
            RightValue = Format(type, rightValue);
        }

        #endregion

        #region Formatting

        private static string Format(Type type, float value) {
            return type switch {
                Type.TD => FormatTimeDependence(value),
                Type.Pre => FormatSwingPercentage(value),
                Type.Post => FormatSwingPercentage(value),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static string FormatTimeDependence(float value) {
            return $"{value:F3}";
        }

        private static string FormatSwingPercentage(float value) {
            return $"{value * 100f:F2}<size=60%>%";
        }

        #endregion

        #region Label

        private string _label = "";

        [UIValue("label"), UsedImplicitly]
        private string Label {
            get => _label;
            set {
                if (_label.Equals(value)) return;
                _label = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region LeftValue

        private string _leftValue = "";

        [UIValue("left-value"), UsedImplicitly]
        private string LeftValue {
            get => _leftValue;
            set {
                if (_leftValue.Equals(value)) return;
                _leftValue = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region RightValue

        private string _rightValue = "";

        [UIValue("right-value"), UsedImplicitly]
        private string RightValue {
            get => _rightValue;
            set {
                if (_rightValue.Equals(value)) return;
                _rightValue = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Background

        [UIComponent("background"), UsedImplicitly]
        private ImageView _backgroundImage;

        private void SetMaterials() {
            _backgroundImage.material = BundleLoader.AccDetailsRowMaterial;
            _backgroundImage.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        }

        #endregion
    }
}