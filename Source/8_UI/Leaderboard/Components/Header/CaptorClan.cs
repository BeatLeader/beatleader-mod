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
                    _captorClanText.text = "⚔ Contested";
                    _captorClanText.faceColor = new Color32(192, 192, 192, 255);
                    CaptorClanHover = "Set a score on this leaderboard to break the tie and capture it for your clan!";
                } else if (data.Clan?.tag == null) {
                    // Map is not captured
                    _captorClanTag.Clear();
                    _captorClanText.text = "👑 Uncaptured";
                    _captorClanText.faceColor = new Color32(255, 255, 255, 255);
                    CaptorClanHover = "Set a score on this leaderboard to capture it for your clan!";
                } else {
                    // Map is captured by a clan
                    _captorClanText.text = "👑 ";
                    _captorClanText.faceColor = new Color32(255, 215, 0, 255);
                    CaptorClanHover = "Clan with the highest weighted PP on this leaderboard!";
                    _captorClanTag.SetValue(data.Clan);
                }
            } else {
                Plugin.Log.Error("Leaderboard Clan Captor missing!");
            }
        }
    }

    #endregion
}