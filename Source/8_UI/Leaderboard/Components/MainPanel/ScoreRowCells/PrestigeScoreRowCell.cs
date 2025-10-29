using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class PrestigeScoreRowCell : AbstractScoreRowCell {
        #region Components

        private const float Size = 3.0f;

        [UIValue("prestige-icon"), UsedImplicitly]
        private PrestigeIcon _prestigeIcon;
        private void Awake() {
            _prestigeIcon = Instantiate<PrestigeIcon>(transform);
        }

        #endregion

        #region Implementation

        public override void SetValue(object? value) {
            if (value is int prestige) {
                _prestigeIcon.SetPrestige(prestige);
                isEmpty = false;
            } else {
                isEmpty = true;
            }
        }

        public override void SetAlpha(float value) {
            _prestigeIcon.SetAlpha(value);
        }

        protected override float CalculatePreferredWidth() {
            return Size;
        }

        #endregion
    }
}