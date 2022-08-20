using System;
using BeatLeader.DataManager;
using System.Diagnostics;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class BeatLeaderInfo : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            InitializePlaylistButtons();

            LeaderboardEvents.LogoWasPressedEvent += ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent += OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;
            ModVersionChecker.IsUpToDateChangedEvent += OnModIsUpToDateChanged;
            PlaylistsManager.PlaylistStateChangedEvent += OnPlaylistStateChanged;
            PlaylistsManager.PlaylistUpdateStartedEvent += OnPlaylistUpdateStarted;
            PlaylistsManager.PlaylistUpdateFinishedEvent += OnPlaylistUpdateFinished;
            OculusMigrationManager.IsMigrationRequiredChangedEvent += OnOculusMigrationRequiredChanged;

            OnPlaylistStateChanged(
                PlaylistsManager.PlaylistType.Nominated,
                PlaylistsManager.GetPlaylistState(PlaylistsManager.PlaylistType.Nominated)
            );
            OnPlaylistStateChanged(
                PlaylistsManager.PlaylistType.Qualified,
                PlaylistsManager.GetPlaylistState(PlaylistsManager.PlaylistType.Qualified)
            );
            OnPlaylistStateChanged(
                PlaylistsManager.PlaylistType.Ranked,
                PlaylistsManager.GetPlaylistState(PlaylistsManager.PlaylistType.Ranked)
            );

            OnModIsUpToDateChanged(ModVersionChecker.IsUpToDate);
            OnOculusMigrationRequiredChanged(OculusMigrationManager.IsMigrationRequired);
        }

        protected override void OnDispose() {
            LeaderboardEvents.LogoWasPressedEvent -= ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent -= OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
            ModVersionChecker.IsUpToDateChangedEvent -= OnModIsUpToDateChanged;
            PlaylistsManager.PlaylistStateChangedEvent -= OnPlaylistStateChanged;
            PlaylistsManager.PlaylistUpdateStartedEvent -= OnPlaylistUpdateStarted;
            PlaylistsManager.PlaylistUpdateFinishedEvent -= OnPlaylistUpdateFinished;
            OculusMigrationManager.IsMigrationRequiredChangedEvent -= OnOculusMigrationRequiredChanged;
        }

        #endregion

        #region Events

        private void OnPlaylistUpdateStarted(PlaylistsManager.PlaylistType playlistType) {
            if (!TryGetPlaylistButtonForType(playlistType, out var button)) return;
            button.interactable = false;
        }

        private void OnPlaylistUpdateFinished(PlaylistsManager.PlaylistType playlistType) {
            if (!TryGetPlaylistButtonForType(playlistType, out var button)) return;
            button.interactable = true;
        }

        private void OnPlaylistStateChanged(PlaylistsManager.PlaylistType playlistType, PlaylistsManager.PlaylistState playlistState) {
            if (!TryGetPlaylistButtonForType(playlistType, out var button)) return;
            UpdatePlaylistButton(button, playlistType, playlistState);
        }

        private void OnModIsUpToDateChanged(bool value) {
            VersionUpdateButtonActive = !value;
            VersionInfoText = value ? UpdatedModVersionText : OutdatedModVersionText;
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

        [UIComponent("modal"), UsedImplicitly] private ModalView _modal;

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

        #region PlaylistButtons

        [UIComponent("nominated-playlist-button"), UsedImplicitly] private Button _nominatedPlaylistButton;
        [UIComponent("qualified-playlist-button"), UsedImplicitly] private Button _qualifiedPlaylistButton;
        [UIComponent("ranked-playlist-button"), UsedImplicitly] private Button _rankedPlaylistButton;

        private bool TryGetPlaylistButtonForType(PlaylistsManager.PlaylistType playlistType, out Button button) {
            switch (playlistType) {
                case PlaylistsManager.PlaylistType.Nominated:
                    button = _nominatedPlaylistButton;
                    return true;
                case PlaylistsManager.PlaylistType.Qualified:
                    button = _qualifiedPlaylistButton;
                    return true;
                case PlaylistsManager.PlaylistType.Ranked:
                    button = _rankedPlaylistButton;
                    return true;
                default:
                    button = null;
                    return false;
            }
        }

        private void InitializePlaylistButtons() {
            _nominatedPlaylistButton.onClick.AddListener(() => LeaderboardEvents.NotifyPlaylistUpdateButtonWasPressed(PlaylistsManager.PlaylistType.Nominated));
            _qualifiedPlaylistButton.onClick.AddListener(() => LeaderboardEvents.NotifyPlaylistUpdateButtonWasPressed(PlaylistsManager.PlaylistType.Qualified));
            _rankedPlaylistButton.onClick.AddListener(() => LeaderboardEvents.NotifyPlaylistUpdateButtonWasPressed(PlaylistsManager.PlaylistType.Ranked));
        }

        private static void UpdatePlaylistButton(Button button, PlaylistsManager.PlaylistType playlistType, PlaylistsManager.PlaylistState playlistState) {
            var prefix = playlistState switch {
                PlaylistsManager.PlaylistState.NotFound => "<color=#FF8888>",
                PlaylistsManager.PlaylistState.Outdated => "<color=#FFFF88>",
                PlaylistsManager.PlaylistState.UpToDate => "<color=#88FF88>",
                _ => throw new ArgumentOutOfRangeException(nameof(playlistState), playlistState, null)
            };

            button.GetComponentInChildren<TextMeshProUGUI>().text = $"{prefix}{playlistType}";
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