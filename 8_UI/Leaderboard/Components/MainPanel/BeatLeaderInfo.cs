using BeatLeader.DataManager;
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
            ModVersionChecker.IsUpToDateChangedEvent += OnModIsUpToDateChanged;
            RankedPlaylistManager.IsUpToDateChangedEvent += OnPlaylistIsUpToDateChanged;
            RankedPlaylistManager.PlaylistUpdateStartedEvent += OnPlaylistUpdateStarted;
            RankedPlaylistManager.PlaylistUpdateFinishedEvent += OnPlaylistUpdateFinished;
            OculusMigrationManager.IsMigrationRequiredChangedEvent += OnOculusMigrationRequiredChanged;

            OnModIsUpToDateChanged(ModVersionChecker.IsUpToDate);
            OnPlaylistIsUpToDateChanged(RankedPlaylistManager.IsUpToDate);
            OnOculusMigrationRequiredChanged(OculusMigrationManager.IsMigrationRequired);
        }

        protected override void OnDispose() {
            LeaderboardEvents.LogoWasPressedEvent -= ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent -= OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
            ModVersionChecker.IsUpToDateChangedEvent -= OnModIsUpToDateChanged;
            RankedPlaylistManager.IsUpToDateChangedEvent -= OnPlaylistIsUpToDateChanged;
            RankedPlaylistManager.PlaylistUpdateStartedEvent -= OnPlaylistUpdateStarted;
            RankedPlaylistManager.PlaylistUpdateFinishedEvent -= OnPlaylistUpdateFinished;
            OculusMigrationManager.IsMigrationRequiredChangedEvent -= OnOculusMigrationRequiredChanged;
        }

        #endregion

        #region Events

        private void OnPlaylistUpdateStarted() {
            PlaylistUpdateButtonInteractable = false;
        }

        private void OnPlaylistUpdateFinished() {
            PlaylistUpdateButtonInteractable = true;
        }

        private void OnModIsUpToDateChanged(bool value) {
            VersionUpdateButtonActive = !value;
            VersionInfoText = value ? UpdatedModVersionText : OutdatedModVersionText;
        }

        private void OnPlaylistIsUpToDateChanged(bool value) {
            PlaylistUpdateButtonActive = !value;
            PlaylistInfoText = value ? UpdatedPlaylistText : OutdatedPlaylistText;
        }

        private void OnOculusMigrationRequiredChanged(bool value) {
            OculusMigrationButtonActive = value;
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

        #region ModVersion

        private const string OutdatedModVersionText = "<color=#FF8888>Mod version is outdated!";
        private const string UpdatedModVersionText = "<color=#88FF88>Mod version is up to date!";

        private string _versionInfoText = "";

        [UIValue("version-info-text"), UsedImplicitly]
        private string VersionInfoText {
            get => _versionInfoText;
            set {
                if (_versionInfoText.Equals(value)) return;
                _versionInfoText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _versionUpdateButtonActive;

        [UIValue("version-update-button-active"), UsedImplicitly]
        private bool VersionUpdateButtonActive {
            get => _versionUpdateButtonActive;
            set {
                if (_versionUpdateButtonActive.Equals(value)) return;
                _versionUpdateButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("version-update-button-on-click"), UsedImplicitly]
        private void VersionUpdateButtonOnClick() {
            if (ModVersionChecker.LatestReleaseInfo.link == null) return;
            EnvironmentUtils.OpenBrowserPage(ModVersionChecker.LatestReleaseInfo.link);
        }

        #endregion

        #region RankedPlaylist

        private const string OutdatedPlaylistText = "<color=#FFFF88>Playlist update available!";
        private const string UpdatedPlaylistText = "<color=#88FF88>Playlist is up to date!";

        private string _playlistInfoText = "";

        [UIValue("playlist-info-text"), UsedImplicitly]
        private string PlaylistInfoText {
            get => _playlistInfoText;
            set {
                if (_playlistInfoText.Equals(value)) return;
                _playlistInfoText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _playlistUpdateButtonActive;

        [UIValue("playlist-update-button-active"), UsedImplicitly]
        private bool PlaylistUpdateButtonActive {
            get => _playlistUpdateButtonActive;
            set {
                if (_playlistUpdateButtonActive.Equals(value)) return;
                _playlistUpdateButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        private bool _playlistUpdateButtonInteractable = true;

        [UIValue("playlist-update-button-interactable"), UsedImplicitly]
        private bool PlaylistUpdateButtonInteractable {
            get => _playlistUpdateButtonInteractable;
            set {
                if (_playlistUpdateButtonInteractable.Equals(value)) return;
                _playlistUpdateButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("playlist-update-button-on-click"), UsedImplicitly]
        private void PlaylistUpdateButtonOnClick() {
            LeaderboardEvents.NotifyRankedPlaylistUpdateButtonWasPressed();
        }

        #endregion

        #region OculusMigrationButton

        private bool _oculusMigrationButtonActive;

        [UIValue("oculus-migration-button-active"), UsedImplicitly]
        private bool OculusMigrationButtonActive {
            get => _oculusMigrationButtonActive;
            set {
                if (_oculusMigrationButtonActive.Equals(value)) return;
                _oculusMigrationButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("oculus-migration-button-on-click"), UsedImplicitly]
        private void OculusMigrationButtonOnClick() {
            LeaderboardEvents.NotifyOculusMigrationButtonWasPressed();
        }

        #endregion

        #region Community Buttons

        [UIAction("website-on-click"), UsedImplicitly]
        private void WebsiteOnClick() {
            EnvironmentUtils.OpenBrowserPage("https://www.beatleader.xyz/dashboard");
        }

        [UIAction("discord-on-click"), UsedImplicitly]
        private void DiscordOnClick() {
            EnvironmentUtils.OpenBrowserPage("https://discord.gg/2RG5YVqtG6");
        }

        [UIAction("patreon-on-click"), UsedImplicitly]
        private void PatreonOnClick() {
            EnvironmentUtils.OpenBrowserPage("https://www.patreon.com/beatleader");
        }

        #endregion
    }
}