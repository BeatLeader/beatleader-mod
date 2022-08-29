using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class VotingPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("rankability-selector"), UsedImplicitly]
        private RankabilitySelector _rankabilitySelector;

        [UIValue("stars-selector"), UsedImplicitly]
        private StarsSelector _starsSelector;

        [UIValue("map-type-selector"), UsedImplicitly]
        private MapTypeSelector _mapTypeSelector;

        private void Awake() {
            _rankabilitySelector = Instantiate<RankabilitySelector>(transform);
            _starsSelector = Instantiate<StarsSelector>(transform);
            _mapTypeSelector = Instantiate<MapTypeSelector>(transform);
        }

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.VotingWasPressedEvent += OnVotingWasPressed;
            LeaderboardEvents.HideAllOtherModalsEvent += OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;

            _rankabilitySelector.OnStateChangedEvent += OnRankabilitySelectorStateChanged;
            OnRankabilitySelectorStateChanged(_rankabilitySelector.CurrentState);
        }

        protected override void OnDispose() {
            LeaderboardEvents.VotingWasPressedEvent -= OnVotingWasPressed;
            LeaderboardEvents.HideAllOtherModalsEvent -= OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;

            _rankabilitySelector.OnStateChangedEvent -= OnRankabilitySelectorStateChanged;
        }

        #endregion

        #region Events

        private void OnVotingWasPressed() {
            _rankabilitySelector.Reset();
            _starsSelector.Reset();
            _mapTypeSelector.Reset();
            Page = FirstPage;
            ShowModal();
        }

        private void OnRankabilitySelectorStateChanged(RankabilitySelector.State state) {
            CanSubmit = state != RankabilitySelector.State.Undecided;
            _showOptional = state == RankabilitySelector.State.ForRank;
            UpdatePaginationButtons();
        }

        private void OnHideModalsEvent(ModalView except) {
            if (_modal == null || _modal.Equals(except)) return;
            _modal.Hide(false);
        }

        private void OnLeaderboardVisibilityChanged(bool isVisible) {
            if (!isVisible) HideAnimated();
        }

        #endregion

        #region Modal

        [UIComponent("modal"), UsedImplicitly]
        private ModalView _modal;

        private void ShowModal() {
            if (_modal == null) return;
            LeaderboardEvents.FireHideAllOtherModalsEvent(_modal);
            _modal.Show(true, true);
        }

        private void HideAnimated() {
            if (_modal == null) return;
            _modal.Hide(true);
        }

        #endregion

        #region Pagination

        private const int FirstPage = 0;
        private const int LastPage = 1;
        private int _page = FirstPage;
        private bool _showOptional;

        private int Page {
            get => _page;
            set {
                if (value < FirstPage) value = FirstPage;
                if (value > LastPage) value = LastPage;
                if (_page.Equals(value)) return;
                _page = value;
                Page0Active = _page == 0;
                Page1Active = _page == 1;
                UpdatePaginationButtons();
            }
        }

        private void UpdatePaginationButtons() {
            HasPrevPage = _page > FirstPage;
            HasNextPage = _showOptional && _page < LastPage;
        }

        [UIAction("prev-on-click"), UsedImplicitly]
        private void PrevOnClick() {
            Page -= 1;
        }

        [UIAction("next-on-click"), UsedImplicitly]
        private void NextOnClick() {
            Page += 1;
        }

        private bool _hasNextPage = true;

        [UIValue("has-next-page"), UsedImplicitly]
        private bool HasNextPage {
            get => _hasNextPage;
            set {
                if (_hasNextPage.Equals(value)) return;
                _hasNextPage = value;
                NotifyPropertyChanged();
            }
        }

        private bool _hasPrevPage;

        [UIValue("has-prev-page"), UsedImplicitly]
        private bool HasPrevPage {
            get => _hasPrevPage;
            set {
                if (_hasPrevPage.Equals(value)) return;
                _hasPrevPage = value;
                NotifyPropertyChanged();
            }
        }

        private bool _page0Active = true;

        [UIValue("page-0-active"), UsedImplicitly]
        private bool Page0Active {
            get => _page0Active;
            set {
                if (_page0Active.Equals(value)) return;
                _page0Active = value;
                NotifyPropertyChanged();
            }
        }

        private bool _page1Active;

        [UIValue("page-1-active"), UsedImplicitly]
        private bool Page1Active {
            get => _page1Active;
            set {
                if (_page1Active.Equals(value)) return;
                _page1Active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region SubmitButton

        private bool _canSubmit;

        [UIValue("can-submit"), UsedImplicitly]
        private bool CanSubmit {
            get => _canSubmit;
            set {
                if (_canSubmit.Equals(value)) return;
                _canSubmit = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("submit-on-click"), UsedImplicitly]
        private void SubmitOnClick() {
            HideAnimated();
            
            switch (_rankabilitySelector.CurrentState) {
                case RankabilitySelector.State.ForRank:
                    LeaderboardEvents.SubmitVote(new Vote(_rankabilitySelector.FloatValue, _starsSelector.Value, _mapTypeSelector.CurrentState));
                    break;
                case RankabilitySelector.State.NotForRank:
                    LeaderboardEvents.SubmitVote(new Vote(_rankabilitySelector.FloatValue));
                    break;
                case RankabilitySelector.State.Undecided:
                default: return;
            }
        }

        #endregion
    }
}