using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;

namespace BeatLeader.Components {
    internal class ClansScoreRowCell : AbstractScoreRowCell {
        #region Components

        [UIValue("clan-tags"), UsedImplicitly]
        private ClanTags _clanTags;

        private void Awake() {
            _clanTags = Instantiate<ClanTags>(transform);
        }

        #endregion

        #region SetValues

        public void SetValues(Clan[] clans) {
            _clanTags.SetClans(clans);
            isEmpty = clans.IsEmpty();
        }

        public override void SetAlpha(float value) {
            _clanTags.SetAlpha(value);
        }

        #endregion

        #region CalculatePreferredWidth

        protected override float CalculatePreferredWidth() {
            return _clanTags.CalculatePreferredWidth();
        }

        #endregion
    }
}