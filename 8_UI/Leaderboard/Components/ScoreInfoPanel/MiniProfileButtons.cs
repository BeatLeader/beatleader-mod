using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Components {
    internal class MiniProfileButtons : ReeUIComponentV2 {
        #region Components

        [UIValue("friend-button"), UsedImplicitly]
        private MiniProfileButton _friendButton;

        [UIValue("hide-button"), UsedImplicitly]
        private MiniProfileButton _hideButton;

        [UIValue("profile-button"), UsedImplicitly]
        private MiniProfileButton _profileButton;

        [UIValue("twitter-button"), UsedImplicitly]
        private MiniProfileButton _twitterButton;

        [UIValue("twitch-button"), UsedImplicitly]
        private MiniProfileButton _twitchButton;

        [UIValue("youtube-button"), UsedImplicitly]
        private MiniProfileButton _youtubeButton;

        private void Awake() {
            _friendButton = Instantiate<MiniProfileButton>(transform);
            _hideButton = Instantiate<MiniProfileButton>(transform);
            _profileButton = Instantiate<MiniProfileButton>(transform);

            _twitterButton = Instantiate<MiniProfileButton>(transform);
            _twitchButton = Instantiate<MiniProfileButton>(transform);
            _youtubeButton = Instantiate<MiniProfileButton>(transform);

            InitializeFriendButton();
            InitializeHideButton();
            InitializeLinkButtons();
        }

        #endregion

        #region Init/Dispose

        protected override void OnInitialize() {
            AddFriendRequest.AddStateListener(OnAddFriendRequestStateChanged);
            RemoveFriendRequest.AddStateListener(OnRemoveFriendRequestStateChanged);
            ProfileManager.FriendsUpdatedEvent += UpdateFriendButton;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdateHideButton;
            UpdateLayout();
        }

        protected override void OnDispose() {
            AddFriendRequest.RemoveStateListener(OnAddFriendRequestStateChanged);
            RemoveFriendRequest.RemoveStateListener(OnRemoveFriendRequestStateChanged);
            ProfileManager.FriendsUpdatedEvent -= UpdateFriendButton;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdateHideButton;
        }

        #endregion

        #region Player

        private Player _player;
        private bool _isMe;

        public void SetPlayer(Player player) {
            _player = player;
            _isMe = ProfileManager.IsCurrentPlayer(player);

            UpdateFriendButton();
            UpdateHideButton();
            UpdateLinkButtons();
            UpdateRoles();
        }

        #endregion

        #region Friend button

        private bool _isFriend;
        private bool _addingFriend;
        private bool _removingFriend;

        private void InitializeFriendButton() {
            _friendButton.Setup(BundleLoader.GraphIcon, true);
            _friendButton.OnClick += FriendButtonOnClick;
        }

        private void OnAddFriendRequestStateChanged(API.RequestState state, Player result, string failReason) {
            _addingFriend = state is API.RequestState.Started;
            UpdateFriendButton();
        }

        private void OnRemoveFriendRequestStateChanged(API.RequestState state, Player result, string failReason) {
            _removingFriend = state is API.RequestState.Started;
            UpdateFriendButton();
        }

        private void UpdateFriendButton() {
            if (_addingFriend || _removingFriend) {
                _friendButton.SetState(MiniProfileButton.State.Inactive);
                return;
            }

            if (_isMe) {
                _friendButton.SetState(MiniProfileButton.State.Hidden);
                return;
            }

            _isFriend = ProfileManager.IsFriend(_player);
            _friendButton.SetState(MiniProfileButton.State.Active);
            _friendButton.SetLabel(_isFriend ? "Remove friend" : "Add friend");
        }

        private void FriendButtonOnClick() {
            if (_isFriend) {
                LeaderboardEvents.NotifyRemoveFriendWasPressed(_player);
            } else {
                LeaderboardEvents.NotifyAddFriendWasPressed(_player);
            }
        }

        #endregion

        #region Hide button

        private bool _isHidden;

        private void InitializeHideButton() {
            _hideButton.Setup(BundleLoader.GridIcon, true);
            _hideButton.OnClick += HideButtonOnClick;
        }

        private void UpdateHideButton() {
            if (_isMe) {
                _hideButton.SetState(MiniProfileButton.State.Hidden);
                return;
            }

            _hideButton.SetState(MiniProfileButton.State.Active);
            _isHidden = HiddenPlayersCache.IsHidden(_player);
            _hideButton.SetLabel(_isHidden ? "Reveal player" : "Hide player");
        }

        private void HideButtonOnClick() {
            if (_isHidden) {
                HiddenPlayersCache.RevealPlayer(_player);
            } else {
                HiddenPlayersCache.HidePlayer(_player);
            }
        }

        #endregion

        #region Link buttons

        private ServiceIntegration _twitterIntegration;
        private ServiceIntegration _twitchIntegration;
        private ServiceIntegration _youtubeIntegration;

        private void InitializeLinkButtons() {
            _profileButton.Setup(BundleLoader.DetailsIcon, true);
            _twitterButton.Setup(BundleLoader.LocationIcon, false);
            _twitchButton.Setup(BundleLoader.ModifiersIcon, false);
            _youtubeButton.Setup(BundleLoader.FileError, false);

            _profileButton.SetLabel("Open profile");
            _twitterButton.SetLabel("Twitter");
            _twitchButton.SetLabel("Twitch");
            _youtubeButton.SetLabel("Youtube");

            _profileButton.OnClick += OnProfileButtonClick;
            _twitterButton.OnClick += OnTwitterButtonClick;
            _twitchButton.OnClick += OnTwitchButtonClick;
            _youtubeButton.OnClick += OnYoutubeButtonClick;
        }

        private void UpdateLinkButtons() {
            UpdateIntegrations();

            _twitterButton.SetState(_twitterIntegration == null ? MiniProfileButton.State.Inactive : MiniProfileButton.State.Active);
            _twitchButton.SetState(_twitchIntegration == null ? MiniProfileButton.State.Inactive : MiniProfileButton.State.Active);
            _youtubeButton.SetState(_youtubeIntegration == null ? MiniProfileButton.State.Inactive : MiniProfileButton.State.Active);
        }

        private void UpdateIntegrations() {
            _twitterIntegration = null;
            _twitchIntegration = null;
            _youtubeIntegration = null;

            if (_player.socials == null) return;

            foreach (var integration in _player.socials) {
                switch (integration.service) {
                    case "twitter":
                        _twitterIntegration = integration;
                        break;
                    case "twitch":
                        _twitchIntegration = integration;
                        break;
                    case "youtube":
                        _youtubeIntegration = integration;
                        break;
                }
            }
        }

        private void OnProfileButtonClick() {
            EnvironmentUtils.OpenBrowserPage(BLConstants.PlayerProfilePage(_player.id));
        }

        private void OnTwitterButtonClick() {
            if (_twitterIntegration == null) return;
            EnvironmentUtils.OpenBrowserPage(_twitterIntegration.link);
        }

        private void OnTwitchButtonClick() {
            if (_twitchIntegration == null) return;
            EnvironmentUtils.OpenBrowserPage(_twitchIntegration.link);
        }

        private void OnYoutubeButtonClick() {
            if (_youtubeIntegration == null) return;
            EnvironmentUtils.OpenBrowserPage(_youtubeIntegration.link);
        }

        #endregion

        #region Layout

        private const float Radius = 16f;
        private const float StepRadians = Mathf.Deg2Rad * 20;
        private const float LeftOffsetRadians = Mathf.Deg2Rad * 180;
        private const float RightOffsetRadians = Mathf.Deg2Rad * 0;

        [UIComponent("left-buttons-root"), UsedImplicitly]
        private Transform _leftButtonsRoot;

        [UIComponent("right-buttons-root"), UsedImplicitly]
        private Transform _rightButtonsRoot;

        private void UpdateRoles() {
            var playerRoles = FormatUtils.ParsePlayerRoles(_player.role);
            var showSocials = playerRoles.Any(role => role.IsAnySupporter());
            _rightButtonsRoot.gameObject.SetActive(showSocials);
        }

        private void UpdateLayout() {
            AlignChild(_leftButtonsRoot, LeftOffsetRadians, StepRadians);
            AlignChild(_rightButtonsRoot, RightOffsetRadians, -StepRadians);
        }

        private static void AlignChild(Transform parent, float offsetRadians, float stepRadians) {
            var angleRadians = offsetRadians - (parent.childCount - 1) * stepRadians / 2;
            for (var i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);
                child.localPosition = new Vector3(
                    Mathf.Cos(angleRadians) * Radius,
                    Mathf.Sin(angleRadians) * Radius,
                    0
                );
                angleRadians += stepRadians;
            }
        }

        #endregion
    }
}