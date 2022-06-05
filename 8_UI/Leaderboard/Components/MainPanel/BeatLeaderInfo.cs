using System.Diagnostics;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class BeatLeaderInfo : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.LogoWasPressedEvent += ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent += OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;
        }

        protected override void OnDispose() {
            LeaderboardEvents.LogoWasPressedEvent -= ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent -= OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
        }

        #endregion

        #region Events
        
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