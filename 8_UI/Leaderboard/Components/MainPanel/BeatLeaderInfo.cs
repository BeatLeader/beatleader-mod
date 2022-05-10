using System.Diagnostics;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class BeatLeaderInfo : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.LogoWasPressedEvent += OnLogoWasPressed;
            LeaderboardEvents.AvatarWasPressedEvent += HideInstantly;
            LeaderboardEvents.SceneTransitionStartedEvent += HideInstantly;
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;
        }

        protected override void OnDispose() {
            LeaderboardEvents.LogoWasPressedEvent -= OnLogoWasPressed;
            LeaderboardEvents.AvatarWasPressedEvent -= HideInstantly;
            LeaderboardEvents.SceneTransitionStartedEvent -= HideInstantly;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
        }

        #endregion

        #region Events

        private void OnLogoWasPressed() {
            ShowModal();
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
            _modal.Show(true, true);
        }

        private void HideInstantly() {
            if (_modal == null) return;
            _modal.Hide(false);
        }

        private void HideAnimated() {
            if (_modal == null) return;
            _modal.Hide(true);
        }

        #endregion

        #region ClickEvents

        [UIAction("website-on-click"), UsedImplicitly]
        private void WebsiteOnClick() {
            Process.Start("explorer.exe", "https://www.beatleader.xyz/dashboard");
        }

        [UIAction("discord-on-click"), UsedImplicitly]
        private void DiscordOnClick() {
            Process.Start("explorer.exe", "https://discord.gg/2RG5YVqtG6");
        }

        [UIAction("patreon-on-click"), UsedImplicitly]
        private void PatreonOnClick() {
            Process.Start("explorer.exe", "https://www.patreon.com/beatleader");
        }

        #endregion
    }
}