using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class MiniProfileButtons : ReeUIComponentV2 {
        #region Components

        [UIValue("friend-button"), UsedImplicitly] private MiniProfileButton _friendButton;
        [UIValue("hide-button"), UsedImplicitly] private MiniProfileButton _hideButton;
        [UIValue("profile-button"), UsedImplicitly] private MiniProfileButton _profileButton;

        [UIValue("twitter-button"), UsedImplicitly] private MiniProfileButton _twitterButton;
        [UIValue("twitch-button"), UsedImplicitly] private MiniProfileButton _twitchButton;
        [UIValue("youtube-button"), UsedImplicitly] private MiniProfileButton _youtubeButton;

        private void Awake() {
            _friendButton = Instantiate<MiniProfileButton>(transform);
            _hideButton = Instantiate<MiniProfileButton>(transform);
            _profileButton = Instantiate<MiniProfileButton>(transform);

            _twitterButton = Instantiate<MiniProfileButton>(transform);
            _twitchButton = Instantiate<MiniProfileButton>(transform);
            _youtubeButton = Instantiate<MiniProfileButton>(transform);

            _friendButton.Setup(BundleLoader.GraphIcon, "Add friend", true);
            _hideButton.Setup(BundleLoader.GridIcon, "Hide player", true);
            _profileButton.Setup(BundleLoader.DetailsIcon, "Open profile", true);

            _twitterButton.Setup(BundleLoader.LocationIcon, "Twitter", false);
            _twitchButton.Setup(BundleLoader.ModifiersIcon, "Twitch", false);
            _youtubeButton.Setup(BundleLoader.FileError, "Youtube", false);
        }

        #endregion

        #region Init/Dispose

        protected override void OnInitialize() {
            UpdateLayout();
        }

        #endregion

        #region Layout

        private const float Radius = 16f;
        private const float StepRadians = Mathf.Deg2Rad * 20;
        private const float LeftOffsetRadians = Mathf.Deg2Rad * 180;
        private const float RightOffsetRadians = Mathf.Deg2Rad * 0;

        [UIComponent("left-buttons-root"), UsedImplicitly] private Transform _leftButtonsRoot;
        [UIComponent("right-buttons-root"), UsedImplicitly] private Transform _rightButtonsRoot;

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