using System.Text;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader.Components {
    internal class ClanMiniProfile : ReeUIComponentV2 {
        #region Components

        [UIComponent("Background"), UsedImplicitly]
        private ImageView _background = null!;

        [UIComponent("Name"), UsedImplicitly]
        private TextMeshProUGUI _name = null!;

        [UIComponent("RankText"), UsedImplicitly]
        private TextMeshProUGUI _rankText = null!;

        [UIValue("Avatar"), UsedImplicitly]
        private PlayerAvatar _avatar = null!;

        [UIValue("Tag"), UsedImplicitly]
        private ClanTag _tag = null!;

        private void Awake() {
            _avatar = Instantiate<PlayerAvatar>(transform);
            _tag = Instantiate<ClanTag>(transform);
        }

        protected override void OnInitialize() {
            _background.raycastTarget = true;
        }

        private void Update() {
            UpdateVisualsIfDirty();
        }

        #endregion

        #region SetClan

        private Clan? _clan;

        public void SetClan(Clan clan) {
            _clan = clan;
            MarkVisualsDirty();
        }

        #endregion

        #region Visuals

        private readonly StringBuilder _rankTextBuilder = new StringBuilder(120);
        private bool _visualsDirty;

        private void MarkVisualsDirty() {
            _visualsDirty = true;
        }

        private void UpdateVisualsIfDirty() {
            if (!_visualsDirty) return;
            _visualsDirty = false;

            if (_clan is null) return;

            _rankTextBuilder.Clear();
            _rankTextBuilder.Append(FormatUtils.FormatRank(_clan.rank, true));
            _rankTextBuilder.Append($"<space=1.0em>{_clan.rankedPoolPercentCaptured * 100:F1}<size=70%><color=#888888>%</size></color>");
            _rankTextBuilder.Append($"<space=0.6em>{_clan.captureLeaderboardsCount}<size=70%><color=#888888> maps</size></color>");

            _name.text = _clan.name;
            _rankText.text = _rankTextBuilder.ToString();
            _avatar.SetAvatar(_clan.avatar, default);
            _tag.SetValue(_clan);
        }

        #endregion
    }
}