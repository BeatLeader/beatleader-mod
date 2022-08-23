using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class PlayerInfo : ReeUIComponentV2 {
        #region Components

        [UIValue("avatar"), UsedImplicitly]
        private PlayerAvatar _avatar;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _countryFlag;

        private void Awake() {
            _avatar = Instantiate<PlayerAvatar>(transform);
            _countryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            UserRequest.AddStateListener(OnProfileRequestStateChanged);
            
            ProfileManager.ProfileUpdatedEvent += OnProfileUpdated;
            if (ProfileManager.HasProfile) OnProfileUpdated(ProfileManager.Profile);
        }

        protected override void OnDispose() {
            UserRequest.RemoveStateListener(OnProfileRequestStateChanged);
            ProfileManager.ProfileUpdatedEvent -= OnProfileUpdated;
        }

        #endregion

        #region Events

        [UIAction("avatar-on-click"), UsedImplicitly]
        private void AvatarOnClick() {
            LeaderboardEvents.NotifyAvatarWasPressed();
        }

        private void OnProfileRequestStateChanged(API.RequestState state, User result, string failReason) {
            switch (state) {
                case API.RequestState.Uninitialized:
                    OnProfileRequestFailed("Error");
                    break;
                case API.RequestState.Failed:
                    OnProfileRequestFailed(failReason);
                    break;
                case API.RequestState.Started:
                    OnProfileRequestStarted();
                    break;
                
                case API.RequestState.Finished:
                default: break;
            }
        }

        private void OnProfileRequestFailed(string reason) {
            NameText = reason;
            StatsActive = false;
        }

        private void OnProfileRequestStarted() {
            NameText = "Loading...";
            StatsActive = false;
        }

        private void OnProfileUpdated(Player player) {
            _countryFlag.SetCountry(player.country);
            _avatar.SetAvatar(player.avatar, FormatUtils.ParsePlayerRoles(player.role));
            NameText = FormatUtils.FormatUserName(player.name);
            GlobalRankText = FormatUtils.FormatRank(player.rank, true);
            CountryRankText = FormatUtils.FormatRank(player.countryRank, true);
            PpText = FormatUtils.FormatPP(player.pp);
            StatsActive = true;
        }

        #endregion

        #region StatsActive

        private bool _statsActive;

        [UIValue("stats-active"), UsedImplicitly]
        public bool StatsActive {
            get => _statsActive;
            set {
                if (_statsActive.Equals(value)) return;
                _statsActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region NameText

        private string _nameText = "";

        [UIValue("name-text"), UsedImplicitly]
        public string NameText {
            get => _nameText;
            set {
                if (_nameText.Equals(value)) return;
                _nameText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region GlobalRankText

        private string _globalRankText = "";

        [UIValue("global-rank-text"), UsedImplicitly]
        public string GlobalRankText {
            get => _globalRankText;
            set {
                if (_globalRankText.Equals(value)) return;
                _globalRankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region CountryRankText

        private string _countryRankText = "";

        [UIValue("country-rank-text"), UsedImplicitly]
        public string CountryRankText {
            get => _countryRankText;
            set {
                if (_countryRankText.Equals(value)) return;
                _countryRankText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PpText

        private string _ppText = "";

        [UIValue("pp-text"), UsedImplicitly]
        public string PpText {
            get => _ppText;
            set {
                if (_ppText.Equals(value)) return;
                _ppText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}