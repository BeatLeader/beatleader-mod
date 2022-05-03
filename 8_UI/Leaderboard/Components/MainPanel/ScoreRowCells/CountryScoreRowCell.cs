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

        public void SetValue(string value) {
            _countryFlag.SetCountry(value);
            IsEmpty = false;
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