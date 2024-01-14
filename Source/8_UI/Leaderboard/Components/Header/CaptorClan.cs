using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class CaptorClan : ReeUIComponentV2 {
        #region Components

        [UIComponent("captor-clan-text"), UsedImplicitly]
        private TextMeshProUGUI _captorClanText;

        [UIValue("clan-tag"), UsedImplicitly]
        private ClanTag _captorClanTag;

        private string _captorClanHover = "";

        [UIValue("captor-clan-hover"), UsedImplicitly]
        private string CaptorClanHover {
            get => _captorClanHover;
            set {
                if (_captorClanHover.Equals(value)) return;
                _captorClanHover = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Init/Dispose

        private void Awake() {
            _captorClanTag = Instantiate<ClanTag>(transform, false);
        }

        protected override void OnInitialize() {
            _captorClanTag.CalculatePreferredWidth();

            SimpleClickHandler.Custom(Content.gameObject, LeaderboardEvents.NotifyCaptorClanWasClickedEvent);
            SmoothHoverController.Scale(Content.gameObject, 1.0f, 1.2f);
        }

        protected override void OnDispose() { }

        #endregion

        #region Set

        public void SetActive(bool value) {
            Content.gameObject.SetActive(value);
        }

        public void SetValues(LeaderboardsCache.LeaderboardCacheEntry data) {
            if (data.Clan != null) {
                if (data.ClanRankingContested) {
                    // Clan Ranking was contested
                    _captorClanTag.Clear();
                    _captorClanText.text = "⚔ <bll>ls-capture-status-contested</bll>";
                    _captorClanText.faceColor = new Color32(192, 192, 192, 255);
                    CaptorClanHover = "<bll>ls-capture-status-contested-hint</bll>";
                } else if (data.Clan?.tag == null) {
                    // Map is not captured
                    _captorClanTag.Clear();
                    _captorClanText.text = "👑 <bll>ls-capture-status-uncaptured</bll>";
                    _captorClanText.faceColor = new Color32(255, 255, 255, 255);
                    CaptorClanHover = "<bll>ls-capture-status-uncaptured-hint</bll>";
                } else {
                    // Map is captured by a clan
                    _captorClanText.text = "👑 ";
                    _captorClanText.faceColor = new Color32(255, 215, 0, 255);
                    CaptorClanHover = BLLocalization.GetTranslation("ls-capture-status-captured-hint")
                        .Replace("<clan>", $"<b>'{data.Clan.name}'</b>");
                    _captorClanTag.SetValue(data.Clan);
                }
            } else {
                Plugin.Log.Error("Leaderboard Clan Captor missing!");
            }
        }
    }

    #endregion
}