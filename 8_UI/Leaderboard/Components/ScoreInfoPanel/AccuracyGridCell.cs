using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyGridCell.bsml")]
    internal class AccuracyGridCell : ReeUIComponent {
        #region SetScore

        public void SetScore(float value) {
            if (value <= 0) {
                Clear();
                return;
            }

            Text = FormatText(value);
        }

        public void Clear() {
            Text = "-";
        }

        #endregion

        #region Formatting

        private static string FormatText(float value) {
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
    }
}