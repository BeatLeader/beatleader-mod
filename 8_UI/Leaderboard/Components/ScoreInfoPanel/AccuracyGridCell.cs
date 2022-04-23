using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyGridCell.bsml")]
    internal class AccuracyGridCell : ReeUIComponent {
        #region Events

        protected override void OnInitialize() {
            SetMaterials();
        }

        #endregion

        #region SetScore

        public void SetScore(float score, float quality) {
            if (score <= 0) {
                Clear();
                return;
            }

            _backgroundImage.color = GetColor(quality);
            Text = FormatText(score);
        }

        private void Clear() {
            _backgroundImage.color = EmptyColor;
            Text = "";
        }

        #endregion

        #region Formatting & Color

        private static readonly Color GoodColor = new Color(0f, 0.2f, 1.0f, 1.0f);
        private static readonly Color BadColor = new Color(0.0f, 0.1f, 0.3f, 0.1f);
        private static readonly Color EmptyColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);

        private static Color GetColor(float quality) {
            var t = quality * quality;
            return Color.Lerp(BadColor, GoodColor, t);
        }
        
        private static string FormatText(float value) {
            return $"{value:F1}";
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

        private void SetMaterials() {
            _backgroundImage.material = BundleLoader.AccGridBackgroundMaterial;
        }

        #endregion
    }
}