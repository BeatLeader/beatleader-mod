using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class StarsSelector : ReeUIComponentV2 {
        #region Value

        private const float Undecided = 0.0f;

        private float _value = Undecided;

        [UIValue("slider-value"), UsedImplicitly]
        public float Value {
            get => _value;
            private set {
                if (_value.Equals(value)) return;
                _value = value;
                NotifyPropertyChanged();
            }
        }

        public void Reset() {
            Value = Undecided;
        }

        #endregion

        #region Formatter

        [UIAction("slider-formatter"), UsedImplicitly]
        private string SliderFormatter(float value) {
            return value > 0 ? $"{value:F1}*" : "skip";
        }

        #endregion
    }
}