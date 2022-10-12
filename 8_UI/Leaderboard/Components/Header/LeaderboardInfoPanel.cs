using System;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;

namespace BeatLeader.Components {
    internal class LeaderboardInfoPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("criteria-checkbox"), UsedImplicitly]
        private QualificationCheckbox _criteriaCheckbox;

        [UIValue("mapper-checkbox"), UsedImplicitly]
        private QualificationCheckbox _mapperCheckbox;

        [UIValue("approval-checkbox"), UsedImplicitly]
        private QualificationCheckbox _approvalCheckbox;

        [UIValue("website-button"), UsedImplicitly]
        private HeaderButton _websiteButton;

        [UIValue("settings-button"), UsedImplicitly]
        private HeaderButton _settingsButton;

        private void Awake() {
            _criteriaCheckbox = Instantiate<QualificationCheckbox>(transform, false);
            _mapperCheckbox = Instantiate<QualificationCheckbox>(transform, false);
            _approvalCheckbox = Instantiate<QualificationCheckbox>(transform, false);
            _websiteButton = Instantiate<HeaderButton>(transform, false);
            _settingsButton = Instantiate<HeaderButton>(transform, false);
        }

        #endregion

        #region Init/Dispose

        protected override void OnInitialize() {
            _websiteButton.Setup(BundleLoader.ProfileIcon);
            _settingsButton.Setup(BundleLoader.GetSpriteFromBundle("SettingsIcon"));
            
            _websiteButton.OnClick += WebsiteButtonOnClick;
            _settingsButton.OnClick += SettingsButtonOnClick;
            LeaderboardsCache.CacheWasChangedEvent += OnCacheWasChanged;
            ProfileManager.RolesUpdatedEvent += OnPlayerRolesUpdated;
            OnPlayerRolesUpdated(ProfileManager.Roles);
            
            ExMachinaRequest.AddStateListener(OnExMachinaRequestStateChanged);
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        protected override void OnDispose() {
            _websiteButton.OnClick -= WebsiteButtonOnClick;
            _settingsButton.OnClick -= SettingsButtonOnClick;
            LeaderboardsCache.CacheWasChangedEvent -= OnCacheWasChanged;
            ProfileManager.RolesUpdatedEvent -= OnPlayerRolesUpdated;
            ExMachinaRequest.RemoveStateListener(OnExMachinaRequestStateChanged);
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        #endregion

        #region Events

        private void OnPlayerRolesUpdated(PlayerRole[] playerRoles) {
            _roles = playerRoles;
            UpdateVisuals();
        }

        private void OnExMachinaRequestStateChanged(API.RequestState state, ExMachinaBasicResponse result, string failReason) {
            if (state is not API.RequestState.Finished) {
                _hasExMachinaRating = false;
                UpdateVisuals();
                return;
            }

            _hasExMachinaRating = true;
            _exMachinaRating = result.balanced;
            UpdateVisuals();
        }

        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            SetBeatmap(beatmap);
        }

        private void OnCacheWasChanged() {
            SetBeatmap(LeaderboardState.SelectedBeatmap);
        }

        #endregion

        #region SetBeatmap

        private PlayerRole[] _roles = Array.Empty<PlayerRole>();
        private RankedStatus _rankedStatus;
        private float _starRating;
        private bool _hasExMachinaRating;
        private float _exMachinaRating;
        private string _websiteLink;

        private void SetBeatmap(IDifficultyBeatmap beatmap) {
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

            _rankedStatus = FormatUtils.GetRankedStatus(data.DifficultyInfo);
            _starRating = data.DifficultyInfo.stars;
            _websiteLink = BLConstants.LeaderboardPage(data.LeaderboardId);

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

            if (qualificationInfo.mapperAllowed) {
                _mapperCheckbox.SetState(QualificationCheckbox.State.Checked);
                _mapperCheckbox.HoverHint = "Allowed by mapper";
            } else {
                _mapperCheckbox.SetState(QualificationCheckbox.State.Neutral);
                _mapperCheckbox.HoverHint = "Awaiting mapper's approval";
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
            StatusActive = _rankedStatus is not RankedStatus.Unknown;
            var starsStr = _starRating > 0 ? $": {_starRating:F1}*" : "";
            StatusText = $"{_rankedStatus}{starsStr}";

            ExMachinaActive = _hasExMachinaRating && _roles.Any(ExMachinaVisibleToRole);
            var exMachinaStarsStr = _exMachinaRating > 0 ? $"{_exMachinaRating:F1}*" : "-";
            ExMachinaText = $"Ex Machina: {exMachinaStarsStr}";

            QualificationActive = _rankedStatus is RankedStatus.Nominated or RankedStatus.Qualified or RankedStatus.Unrankable;
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

        #endregion

        #region StatusPanel

        private bool _statusActive;

        [UIValue("status-active"), UsedImplicitly]
        private bool StatusActive {
            get => _statusActive;
            set {
                if (_statusActive.Equals(value)) return;
                _statusActive = value;
                NotifyPropertyChanged();
            }
        }

        private string _statusText = "";

        [UIValue("status-text"), UsedImplicitly]
        private string StatusText {
            get => _statusText;
            set {
                if (_statusText.Equals(value)) return;
                _statusText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region ExMachinaPanel

        private bool _exMachinaActive;

        [UIValue("ex-machina-active"), UsedImplicitly]
        private bool ExMachinaActive {
            get => _exMachinaActive;
            set {
                if (_exMachinaActive.Equals(value)) return;
                _exMachinaActive = value;
                NotifyPropertyChanged();
            }
        }

        private string _exMachinaText = "";

        [UIValue("ex-machina-text"), UsedImplicitly]
        private string ExMachinaText {
            get => _exMachinaText;
            set {
                if (_exMachinaText.Equals(value)) return;
                _exMachinaText = value;
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
        
        private void WebsiteButtonOnClick() {
            if (_websiteLink == null) return;
            EnvironmentUtils.OpenBrowserPage(_websiteLink);
        }
        
        private void SettingsButtonOnClick() {
            LeaderboardEvents.NotifySettingsButtonWasPressed();
        }

        #endregion
    }
}