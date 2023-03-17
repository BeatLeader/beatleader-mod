using System.Collections.Generic;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
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
            InitializeProfileButton();
            InitializeSocialsButtons();
        }

        #endregion

        #region Init/Dispose

        protected override void OnInitialize() {
            InitializeBackground();

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
            _isMe = ProfileManager.IsCurrentPlayer(player?.id);

            UpdateFriendButton();
            UpdateHideButton();
            UpdateLinkButtons();
            UpdateLayout();
        }

        #endregion

        #region Friend button

        private bool _isFriend;
        private bool _addingFriend;
        private bool _removingFriend;

        private void InitializeFriendButton() {
            _friendButton.Setup(BundleLoader.FriendsIcon, true);
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
            if (_isMe || _addingFriend || _removingFriend) {
                _friendButton.SetLabel("");
                _friendButton.SetState(MiniProfileButton.State.NonInteractable);
                return;
            }

            _isFriend = ProfileManager.IsFriend(_player);

            if (_isFriend) {
                _friendButton.SetLabel("Remove friend");
                _friendButton.SetState(MiniProfileButton.State.InteractableGlowing);
            } else {
                _friendButton.SetLabel("Add friend");
                _friendButton.SetState(MiniProfileButton.State.InteractableFaded);
            }
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
            _hideButton.Setup(BundleLoader.IncognitoIcon, true);
            _hideButton.OnClick += HideButtonOnClick;
        }

        private void UpdateHideButton() {
            if (_isMe) {
                _hideButton.SetLabel("");
                _hideButton.SetState(MiniProfileButton.State.NonInteractable);
                return;
            }

            _isHidden = HiddenPlayersCache.IsHidden(_player);

            if (_isHidden) {
                _hideButton.SetLabel("Reveal player");
                _hideButton.SetState(MiniProfileButton.State.InteractableGlowing);
            } else {
                _hideButton.SetLabel("Hide player");
                _hideButton.SetState(MiniProfileButton.State.InteractableFaded);
            }
        }

        private void HideButtonOnClick() {
            if (_isHidden) {
                HiddenPlayersCache.RevealPlayer(_player);
            } else {
                HiddenPlayersCache.HidePlayer(_player);
            }
        }

        #endregion

        #region ProfileButton

        private void InitializeProfileButton() {
            _profileButton.Setup(BundleLoader.ProfileIcon, true);
            _profileButton.SetLabel("Open profile");
            _profileButton.OnClick += OnProfileButtonClick;
        }

        private void OnProfileButtonClick() {
            EnvironmentUtils.OpenBrowserPage(BLConstants.PlayerProfilePage(_player.id));
        }

        #endregion

        #region Socials buttons

        private static Color TwitterColor => new Color(0.1f, 0.6f, 1.0f, 1.0f);
        private static Color TwitchColor => new Color(0.5f, 0.3f, 1.0f, 1.0f);
        private static Color YoutubeColor => new Color(1.0f, 0.0f, 0.0f, 1.0f);
        

        private readonly Dictionary<string, SocialsButtonInfo> _socialsButtons = new();

        private void InitializeSocialsButtons() {
            InitializeSocialsButton(_twitterButton, "Twitter", BundleLoader.TwitterIcon, TwitterColor);
            InitializeSocialsButton(_twitchButton, "Twitch", BundleLoader.TwitchIcon, TwitchColor);
            InitializeSocialsButton(_youtubeButton, "YouTube", BundleLoader.YoutubeIcon, YoutubeColor);
        }

        private void InitializeSocialsButton(MiniProfileButton button, string serviceName, Sprite sprite, Color color) {
            var info = new SocialsButtonInfo() { Button = button };
            _socialsButtons[serviceName] = info;

            button.Setup(sprite, false);
            button.SetLabel(serviceName);
            button.SetGlowColor(color);
            button.OnClick += () => info.OpenLink();
        }

        private void UpdateLinkButtons() {
            var playerRoles = FormatUtils.ParsePlayerRoles(_player.role);
            var showSocials = playerRoles.Any(role => role.IsAnySupporter());

            foreach (var buttonInfo in _socialsButtons.Values) {
                buttonInfo.Button.SetState(showSocials ? MiniProfileButton.State.NonInteractable : MiniProfileButton.State.Hidden);
                buttonInfo.Integration = null;
            }

            if (!showSocials || _player.socials == null) return;

            foreach (var integration in _player.socials) {
                if (!_socialsButtons.ContainsKey(integration.service)) continue;
                var info = _socialsButtons[integration.service];
                info.Integration = integration;
                info.Button.SetState(MiniProfileButton.State.InteractableGlowing);
            }
        }

        private class SocialsButtonInfo {
            public ServiceIntegration Integration;
            public MiniProfileButton Button;

            public void OpenLink() {
                if (Integration == null) return;
                EnvironmentUtils.OpenBrowserPage(Integration.link);
            }
        }

        #endregion

        #region Layout

        private const float Radius = 16.8f;
        private const float StepRadians = Mathf.Deg2Rad * 18;
        private const float OffsetRadians = Mathf.Deg2Rad * 90;
        private const float BackgroundThickness = 6.1f / 40.0f;

        [UIComponent("left-buttons-root"), UsedImplicitly]
        private Transform _leftButtonsRoot;

        [UIComponent("right-buttons-root"), UsedImplicitly]
        private Transform _rightButtonsRoot;

        private void UpdateLayout() {
            UpdateLayout(_leftButtonsRoot, OffsetRadians, StepRadians, out var leftA, out var leftB);
            UpdateLayout(_rightButtonsRoot, -OffsetRadians, -StepRadians, out var rightA, out var rightB);
            UpdateBackground(leftA, leftB, rightB, rightA);
        }

        private static void UpdateLayout(
            Transform parent, float offsetRadians, float stepRadians,
            out float fromRadians, out float toRadians
        ) {
            var activeCount = 0;
            for (var i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);
                if (child.gameObject.activeSelf) activeCount += 1;
            }

            fromRadians = offsetRadians - (activeCount - 1) * stepRadians / 2;
            toRadians = fromRadians + stepRadians * (activeCount - 1);

            if (activeCount <= 0) {
                parent.gameObject.SetActive(false);
                return;
            }

            parent.gameObject.SetActive(true);

            var angleRadians = Mathf.PI / 2 + fromRadians;
            for (var i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                child.localPosition = new Vector3(
                    Mathf.Cos(angleRadians) * Radius,
                    Mathf.Sin(angleRadians) * Radius,
                    0
                );
                angleRadians += stepRadians;
            }
        }

        #endregion

        #region Backround

        private static readonly Color BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.8f);

        private static readonly int ParamsPropertyId = Shader.PropertyToID("_Params");

        [UIComponent("left-buttons-root"), UsedImplicitly]
        private ImageView _leftBackground;

        [UIComponent("right-buttons-root"), UsedImplicitly]
        private ImageView _rightBackground;

        private Material _leftMaterial;
        private Material _rightMaterial;

        private void InitializeBackground() {
            _leftBackground.material = _leftMaterial = Material.Instantiate(BundleLoader.MiniProfileBackgroundMaterial);
            _rightBackground.material = _rightMaterial = Material.Instantiate(BundleLoader.MiniProfileBackgroundMaterial);
            _leftBackground.color = BackgroundColor;
            _rightBackground.color = BackgroundColor;
        }

        private void UpdateBackground(float leftFrom, float leftTo, float rightFrom, float rightTo) {
            _leftMaterial.SetVector(ParamsPropertyId, GetParamsVector(leftFrom, leftTo, BackgroundThickness));
            _rightMaterial.SetVector(ParamsPropertyId, GetParamsVector(rightFrom, rightTo, BackgroundThickness));
        }

        private static Vector4 GetParamsVector(float fromRadians, float toRadians, float thickness) {
            return new Vector4(fromRadians, toRadians, thickness, 1.0f);
        }

        #endregion
    }
}