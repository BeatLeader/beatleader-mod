using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.UIPatches;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;

namespace BeatLeader.Components {
    internal class LeaderboardInfoPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("criteria-checkbox"), UsedImplicitly]
        private QualificationCheckbox _criteriaCheckbox = null!;

        [UIValue("approval-checkbox"), UsedImplicitly]
        private QualificationCheckbox _approvalCheckbox = null!;

        [UIValue("menu-button"), UsedImplicitly]
        private HeaderButton _menuButton = null!;

        [UIValue("website-button"), UsedImplicitly]
        private HeaderButton _websiteButton = null!;

        [UIValue("settings-button"), UsedImplicitly]
        private HeaderButton _settingsButton = null!;

        [UIValue("map-status"), UsedImplicitly]
        private MapStatus _mapStatus = null!;

        [UIValue("captor-clan"), UsedImplicitly]
        private CaptorClan _captorClan;

        private void Awake() {
            _criteriaCheckbox = Instantiate<QualificationCheckbox>(transform, false);
            _approvalCheckbox = Instantiate<QualificationCheckbox>(transform, false);
            _menuButton = Instantiate<HeaderButton>(transform, false);
            _websiteButton = Instantiate<HeaderButton>(transform, false);
            _settingsButton = Instantiate<HeaderButton>(transform, false);
            _mapStatus = Instantiate<MapStatus>(transform, false);
            _captorClan = Instantiate<CaptorClan>(transform, false);
        }

        #endregion

        #region Init/Dispose

        protected override void OnInitialize() {
            _menuButton.Setup(BundleLoader.Sprites.homeIcon);
            _websiteButton.Setup(BundleLoader.ProfileIcon);
            _settingsButton.Setup(BundleLoader.SettingsIcon);

            _menuButton.OnClick += MenuButtonOnClick;
            _websiteButton.OnClick += WebsiteButtonOnClick;
            _settingsButton.OnClick += SettingsButtonOnClick;
            LeaderboardsCache.CacheWasChangedEvent += OnCacheWasChanged;
            PluginConfig.LeaderboardDisplaySettingsChangedEvent += OnLeaderboardDisplaySettingsChanged;
            EnvironmentManagerPatch.EnvironmentTypeChangedEvent += OnMenuEnvironmentChanged;

            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        protected override void OnDispose() {
            _menuButton.OnClick -= MenuButtonOnClick;
            _websiteButton.OnClick -= WebsiteButtonOnClick;
            _settingsButton.OnClick -= SettingsButtonOnClick;
            LeaderboardsCache.CacheWasChangedEvent -= OnCacheWasChanged;
            EnvironmentManagerPatch.EnvironmentTypeChangedEvent -= OnMenuEnvironmentChanged;
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            PluginConfig.LeaderboardDisplaySettingsChangedEvent -= OnLeaderboardDisplaySettingsChanged;
        }

        #endregion

        #region Events

        private void OnLeaderboardDisplaySettingsChanged(LeaderboardDisplaySettings settings)
        {
            _displayCaptorClan = settings.ClanCaptureDisplay;
            UpdateVisuals();
        }

        private void OnMenuEnvironmentChanged(MenuEnvironmentManager.MenuEnvironmentType type) {
            UpdateVisuals();
        }
        
        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
            SetBeatmap(key);
        }

        private void OnCacheWasChanged() {
            SetBeatmap(LeaderboardState.SelectedBeatmapKey);
        }

        #endregion

        #region SetBeatmap

        private RankedStatus _rankedStatus;
        private DiffInfo _difficultyInfo;
        private bool _displayCaptorClan = PluginConfig.LeaderboardDisplaySettings.ClanCaptureDisplay;
        private string? _websiteLink;

        private void SetBeatmap(BeatmapKey beatmap) {
            if (beatmap == null) {
                _rankedStatus = RankedStatus.Unknown;
                _websiteLink = null;
                UpdateVisuals();
                return;
            }

            var key = LeaderboardKey.FromBeatmap(beatmap);
            if (!LeaderboardsCache.TryGetLeaderboardInfo(key, out var data)) {
                _rankedStatus = RankedStatus.Unknown;
                _websiteLink = null;
                UpdateVisuals();
                return;
            }

            _difficultyInfo = data.DifficultyInfo;
            _rankedStatus = FormatUtils.GetRankedStatus(data.DifficultyInfo);
            _websiteLink = BLConstants.LeaderboardPage(data.LeaderboardId);
            if (_rankedStatus is RankedStatus.Ranked) {
                _captorClan.SetValues(data);
            }

            UpdateCheckboxes(data.QualificationInfo);
            UpdateVisuals();
        }

        #endregion

        #region UpdateCheckboxes

        private void UpdateCheckboxes(QualificationInfo qualificationInfo) {
            string criteriaPostfix;

            if (qualificationInfo.criteriaCommentary == null || qualificationInfo.criteriaCommentary.IsEmpty()) {
                criteriaPostfix = "";
            } else {
                criteriaPostfix = $"<size=80%>\n\n{qualificationInfo.criteriaCommentary}";
            }

            switch (qualificationInfo.criteriaMet) {
                case 1:
                    _criteriaCheckbox.SetState(QualificationCheckbox.State.Checked);
                    _criteriaCheckbox.HoverHint = $"Criteria passed{criteriaPostfix}";
                    break;
                case 2:
                    _criteriaCheckbox.SetState(QualificationCheckbox.State.Failed);
                    _criteriaCheckbox.HoverHint = $"Criteria failed{criteriaPostfix}";
                    break;
                case 3:
                    _criteriaCheckbox.SetState(QualificationCheckbox.State.OnHold);
                    _criteriaCheckbox.HoverHint = $"Criteria on hold{criteriaPostfix}";
                    break;
                default:
                    _criteriaCheckbox.SetState(QualificationCheckbox.State.Neutral);
                    _criteriaCheckbox.HoverHint = $"Awaiting criteria check{criteriaPostfix}";
                    break;
            }

            if (qualificationInfo.approved) {
                _approvalCheckbox.SetState(QualificationCheckbox.State.Checked);
                _approvalCheckbox.HoverHint = "Qualified!";
            } else {
                _approvalCheckbox.SetState(QualificationCheckbox.State.Neutral);
                _approvalCheckbox.HoverHint = "Awaiting RT approval";
            }
        }

        #endregion

        #region UpdateVisuals

        private void UpdateVisuals() {
            _mapStatus.SetActive(_rankedStatus is not RankedStatus.Unknown);
            _mapStatus.SetValues(_rankedStatus, _difficultyInfo);
            _captorClan.SetActive(_displayCaptorClan && _rankedStatus is RankedStatus.Ranked);

            QualificationActive = _rankedStatus is RankedStatus.Nominated or RankedStatus.Qualified or RankedStatus.Unrankable;
            IsMenuButtonActive = EnvironmentManagerPatch.EnvironmentType is not MenuEnvironmentManager.MenuEnvironmentType.Lobby;
        }

        #endregion

        #region Utils

        private static bool ExMachinaVisibleToRole(PlayerRole playerRole) {
            return playerRole.IsAnyAdmin() || playerRole.IsAnyRT() || playerRole.IsAnySupporter();
        }

        private static bool RtToolsVisibleToRole(PlayerRole playerRole) {
            return playerRole.IsAnyAdmin() || playerRole.IsAnyRT();
        }

        #endregion

        #region IsActive
        
        private bool _isMenuButtonActive;
        private bool _isActive;

        [UIValue("is-active"), UsedImplicitly]
        public bool IsActive {
            get => _isActive;
            set {
                if (_isActive.Equals(value)) return;
                _isActive = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("is-menu-button-active"), UsedImplicitly]
        public bool IsMenuButtonActive {
            get => _isMenuButtonActive;
            set {
                if (_isMenuButtonActive.Equals(value)) return;
                _isMenuButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region QualificationPanel

        private bool _qualificationActive;

        [UIValue("qualification-active"), UsedImplicitly]
        private bool QualificationActive {
            get => _qualificationActive;
            set {
                if (_qualificationActive.Equals(value)) return;
                _qualificationActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Buttons

        private void MenuButtonOnClick() {
            LeaderboardEvents.NotifyMenuButtonWasPressed();
        }

        private void WebsiteButtonOnClick() {
            if (_websiteLink == null) return;
            EnvironmentUtils.OpenBrowserPage(_websiteLink);
        }

        private void SettingsButtonOnClick() {
            LeaderboardEvents.NotifyLeaderboardSettingsButtonWasPressed();
        }

        #endregion
    }
}