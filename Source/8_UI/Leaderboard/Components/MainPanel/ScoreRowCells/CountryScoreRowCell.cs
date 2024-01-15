using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class CountryScoreRowCell : AbstractScoreRowCell {
        #region Components

        private const float Size = 3.0f;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _countryFlag;

        private void Awake() {
            _countryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion

        #region Implementation

        public override void SetValue(object? value) {
            if (value is string country) {
                _countryFlag.SetCountry(country);
                isEmpty = false;
            } else {
                isEmpty = true;
            }
        }

        public override void SetAlpha(float value) {
            _countryFlag.SetAlpha(value);
        }

        protected override float CalculatePreferredWidth() {
            return Size;
        }

        #endregion
    }
}