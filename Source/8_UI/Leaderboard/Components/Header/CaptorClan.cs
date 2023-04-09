using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using static AlphabetScrollInfo;

namespace BeatLeader.Components {
    internal class CaptorClan : ReeUIComponentV2 {
        #region Components

        [UIComponent("background"), UsedImplicitly]
        private ImageView _background;

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        private bool _captorClanActive = false;

        [UIValue("captor-clan-active"), UsedImplicitly]
        private bool CaptorClanActive {
            get => _captorClanActive;
            set {
                if (_captorClanActive.Equals(value)) return;
                _captorClanActive = value;
                NotifyPropertyChanged();
            }
        }

        private bool _leaderboardCaptured = false;

        [UIValue("leaderboard-captured"), UsedImplicitly]
        private bool LeaderboardCaptured {
            get => _leaderboardCaptured;
            set {
                if (_leaderboardCaptured.Equals(value)) return;
                _leaderboardCaptured = value;
                NotifyPropertyChanged();
            }
        }

        private string _captorClanText = "";

        [UIValue("captor-clan-text"), UsedImplicitly]
        private string CaptorClanText {
            get => _captorClanText;
            set {
                if (_captorClanText.Equals(value)) return;
                _captorClanText = value;
                NotifyPropertyChanged();
            }
        }

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

        private string _captorClanTextColor = "#FFFFFFDD";

        [UIValue("captor-clan-text-color"), UsedImplicitly]
        private string CaptorClanTextColor {
            get => _captorClanTextColor;
            set {
                if (_captorClanTextColor.Equals(value)) return;
                _captorClanTextColor = value;
                NotifyPropertyChanged();
            }
        }

        //private void InitializeText() {
        //    _textComponent.enableAutoSizing = true;
        //    _textComponent.fontSizeMin = 0.1f;
        //    _textComponent.fontSizeMax = 2.0f;
        //}

        #endregion

        #region Init/Dispose

        protected override void OnInitialize() {
            _background.material = BundleLoader.ClanTagBackgroundMaterial;
        }

        protected override void OnDispose() {
        }

        #endregion

        #region SetValues

        public void SetActive(bool value) {
            // Used to be called on initialize
            _background.material = BundleLoader.ClanTagBackgroundMaterial;
            CaptorClanActive = true;
            _background.gameObject.SetActive(value);
        }

        public void SetValues(LeaderboardsCache.LeaderboardCacheEntry data) {
            if (data.ClanRankingContested) {
                // Clan Ranking was contested
                LeaderboardCaptured = false;
                CaptorClanText = $"⚔ Contested";
                CaptorClanTextColor = "#C0C0C0FF";
                CaptorClanHover = "Set a score on this leaderboard to break the tie and capture it for your clan!";
            } else if (data.ClanRankingInfo.clan.tag == null) {
                // Map is not captured
                LeaderboardCaptured = false;
                CaptorClanText = $"👑 Uncaptured";
                CaptorClanTextColor = "#FFFFFFFF";
                CaptorClanHover = "Set a score on this leaderboard to capture it for your clan!";
            } else {
                // Map is captured by a clan
                LeaderboardCaptured = true;
                CaptorClanText = $"👑 ";
                CaptorClanTextColor = "#FFD700FF";
                CaptorClanHover = "Clan with the highest weighted PP on this leaderboard!";
                UpdateCaptorClan(data.ClanRankingInfo.clan);
            }
        }

        public void UpdateCaptorClan(ClanRankingClanInfo value) {
            _textComponent.text = FormatUtils.FormatClanTag(value.tag);
            SetCaptorClanColor(value.color);
        }

        private float _alpha = 1.0f;
        private Color _color = Color.black;

        private void SetCaptorClanColor(string strColor) {
            ColorUtility.TryParseHtmlString(strColor, out var color);
            var useDarkFont = (color.r * 0.299f + color.g * 0.687f + color.b * 0.114f) > 0.73f;
            _textComponent.color = useDarkFont ? Color.black : Color.white;
            _color = color;
            _background.color = _color.ColorWithAlpha(_alpha);
        }
    }

    #endregion
}