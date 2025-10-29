using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.UI.Hub;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerInfo : ReeUIComponentV2 {
        #region Components

        [UIValue("avatar"), UsedImplicitly]
        private PlayerAvatar _avatar;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _countryFlag;

        private Player player;

        private void Awake() {
            _avatar = Instantiate<PlayerAvatar>(transform);
            _countryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            InitializeMaterial();

            UserRequest.StateChangedEvent += OnProfileRequestStateChanged;
            UploadReplayRequest.StateChangedEvent += OnUploadRequestStateChanged;
            PrestigePanel.PrestigeWasPressedEvent += IncrementPrestigeIcon;
            GlobalSettingsView.ExperienceBarConfigEvent += OnExperienceBarConfigChanged;
            PluginConfig.ScoresContextChangedEvent += ChangeScoreContext;
        }

        protected override void OnDispose() {
            UserRequest.StateChangedEvent -= OnProfileRequestStateChanged;
            UploadReplayRequest.StateChangedEvent -= OnUploadRequestStateChanged;
            PrestigePanel.PrestigeWasPressedEvent -= IncrementPrestigeIcon;
            GlobalSettingsView.ExperienceBarConfigEvent -= OnExperienceBarConfigChanged;
            PluginConfig.ScoresContextChangedEvent -= ChangeScoreContext;
        }

        #endregion

        #region Events

        private void OnUploadRequestStateChanged(WebRequests.IWebRequest<ScoreUploadResponse> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished || instance.Result.Status != ScoreUploadStatus.Uploaded) return;
            OnProfileUpdated(instance.Result.Score.Player);
            player.contextExtensions = instance.Result.Score.Player.contextExtensions;
        }

        private void OnProfileRequestStateChanged(WebRequests.IWebRequest<Player> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Uninitialized:
                    OnProfileRequestFailed("Error");
                    break;
                case WebRequests.RequestState.Failed:
                    OnProfileRequestFailed(failReason);
                    break;
                case WebRequests.RequestState.Started:
                    OnProfileRequestStarted();
                    break;
                case WebRequests.RequestState.Finished:
                    player = instance.Result;
                    OnProfileUpdated(player);
                    break;
                default: return;
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
            _avatar.SetAvatar(player.avatar, player.profileSettings);

            var contextPlayer = player.ContextPlayer(PluginConfig.ScoresContext);
            SwapPrestigeIcon(player.prestige);
            NameText = FormatUtils.FormatUserName(player.name);
            GlobalRankText = FormatUtils.FormatRank(contextPlayer.rank, true);
            CountryRankText = FormatUtils.FormatRank(contextPlayer.countryRank, true);
            PpText = FormatUtils.FormatPP(contextPlayer.pp);
            StatsActive = true;
        }

        #endregion

        private void IncrementPrestigeIcon() {
            SwapPrestigeIcon(player.prestige + 1);
        }

        private void SwapPrestigeIcon(int prestige) {
            switch (prestige) {
                case 0:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon0;
                    break;
                case 1:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon1;
                    break;
                case 2:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon2;
                    break;
                case 3:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon3;
                    break;
                case 4:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon4;
                    break;
                case 5:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon5;
                    break;
                case 6:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon6;
                    break;
                case 7:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon7;
                    break;
                case 8:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon8;
                    break;
                case 9:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon9;
                    break;
                case 10:
                    _prestigeIcon.sprite = BundleLoader.PrestigeIcon10;
                    break;
            }

            if (ConfigFileData.Instance.ExperienceBarEnabled) {
                _prestigeIcon.gameObject.SetActive(true);
            } else {
                _prestigeIcon.gameObject.SetActive(false);
            }
        }

        private void OnExperienceBarConfigChanged(bool enable) {
            if (enable) {
                _prestigeIcon.gameObject.SetActive(true);
            }else {
                _prestigeIcon.gameObject.SetActive(false);
            }
        }

        private void ChangeScoreContext(int context) {
            OnProfileUpdated(player);
        }

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

        #region PrestigeIcon

        [UIComponent("prestige-icon"), UsedImplicitly]
        private ImageView _prestigeIcon = null!;

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

        #region Material

        private Material _materialInstance;

        private void InitializeMaterial() {
            _materialInstance = Material.Instantiate(BundleLoader.PrestigeIconMaterial);
            _prestigeIcon.material = _materialInstance;
            _prestigeIcon.sprite = BundleLoader.TransparentPixel;
            _prestigeIcon.gameObject.SetActive(false);
        }

        #endregion
    }
}