using BeatLeader.DataManager;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class CaptorClan : ReeUIComponentV2 {
        #region Components

        [UIValue("clan-tag"), UsedImplicitly]
        private ClanTag _captorClanTag;

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

        private string _leaderboardCaptorClanStatusText = "";

        [UIValue("captor-clan-text"), UsedImplicitly]
        private string CaptorClanText {
            get => _leaderboardCaptorClanStatusText;
            set {
                if (_leaderboardCaptorClanStatusText.Equals(value)) return;
                _leaderboardCaptorClanStatusText = value;
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

        private string _leaderboardCaptorClanStatusColor = "#FFFFFFDD";

        [UIValue("captor-clan-text-color"), UsedImplicitly]
        private string CaptorClanTextColor {
            get => _leaderboardCaptorClanStatusColor;
            set {
                if (_leaderboardCaptorClanStatusColor.Equals(value)) return;
                _leaderboardCaptorClanStatusColor = value;
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
        }

        protected override void OnDispose() {
        }

        #endregion

        #region Set

        public void SetActive(bool value) {
            CaptorClanActive = value;
        }

        public void SetValues(LeaderboardsCache.LeaderboardCacheEntry data) {
            if (data.ClanRankingContested) {
                // Clan Ranking was contested
                _captorClanTag.Clear();
                CaptorClanText = $"⚔ Contested";
                CaptorClanTextColor = "#C0C0C0FF";
                CaptorClanHover = "Set a score on this leaderboard to break the tie and capture it for your clan!";
            } else if (data.Clan.tag == null) {
                // Map is not captured
                _captorClanTag.Clear();
                CaptorClanText = $"👑 Uncaptured";
                CaptorClanTextColor = "#FFFFFFFF";
                CaptorClanHover = "Set a score on this leaderboard to capture it for your clan!";
            } else {
                // Map is captured by a clan
                CaptorClanText = $"👑 ";
                CaptorClanTextColor = "#FFD700FF";
                CaptorClanHover = "Clan with the highest weighted PP on this leaderboard!";
                _captorClanTag.SetValue(data.Clan);
            }
        }
    }

    #endregion
}