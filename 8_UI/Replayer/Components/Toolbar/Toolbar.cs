using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.UI.BSML_Addons.Components;
using UnityEngine.UI;
using UnityEngine;
using HMUI;
using BeatLeader.Models;
using static HMUI.NoTransitionsButton;
using System;
using BeatLeader.Replayer.Emulation;

namespace BeatLeader.Components {
    internal class Toolbar : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("exit-button-background")] private readonly RectTransform _exitButtonBackground;
        [UIComponent("play-button")] private readonly BetterButton _playButton;
        [UIComponent("exit-button-icon")] private readonly BetterImage _exitButtonIcon;

        [UIValue("timeline")] private Timeline _timeline;

        private NoTransitionsButton _exitButton;

        #endregion

        #region Events

        public event Action? SettingsButtonClickedEvent;

        #endregion

        #region FormattedSongTime

        [UIValue("combined-song-time")]
        public string FormattedSongTime {
            get => _formattedSongTime;
            private set {
                _formattedSongTime = value;
                NotifyPropertyChanged(nameof(FormattedSongTime));
            }
        }

        #endregion

        #region Setup

        private readonly Sprite _playSprite = BundleLoader.PlayIcon;
        private readonly Sprite _pauseSprite = BundleLoader.PauseIcon;
        private readonly Sprite _openedDoorSprite = BundleLoader.OpenedDoorIcon;
        private readonly Sprite _closedDoorSprite = BundleLoader.ClosedDoorIcon;
        private string _formattedSongTime = null!;

        private IReplayPauseController _pauseController = null!;
        private IReplayFinishController _finishController = null!;
        private IBeatmapTimeController _beatmapTimeController = null!;
        private IVirtualPlayersManager _playersManager = null!;
        private ReplayLaunchData _launchData = null!;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IBeatmapTimeController beatmapTimeController,
            IVirtualPlayersManager playersManager,
            ReplayLaunchData launchData) {
            OnDispose();
            _pauseController = pauseController;
            _finishController = finishController;
            _beatmapTimeController = beatmapTimeController;
            _playersManager = playersManager;
            _launchData = launchData;

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _timeline.Setup(playersManager, pauseController, beatmapTimeController, launchData);
        }

        protected override void OnInstantiate() {
            _timeline = Instantiate<Timeline>(transform);
        }

        protected override void OnInitialize() {
            _exitButton = _exitButtonBackground.gameObject.AddComponent<NoTransitionsButton>();
            _exitButton.selectionStateDidChangeEvent += HandleExitButtonSelectionStateChanged;
            _exitButton.navigation = new Navigation() { mode = Navigation.Mode.None };
            _exitButton.onClick.AddListener(HandleExitButtonClicked);
        }

        protected override void OnDispose() {
            if (_pauseController != null)
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
        }

        #endregion

        #region UpdateSongTime

        private void Update() {
            UpdateSongTime();
        }

        private void UpdateSongTime() {
            var failTime = _launchData.IsBattleRoyale ? 0 : _launchData.MainReplay.info.failTime;
            var totalTime = failTime <= 0 ? _beatmapTimeController.SongEndTime : failTime;
            FormattedSongTime = FormatUtils.FormatSongTime(_beatmapTimeController.SongTime, totalTime);
        }

        #endregion

        #region Callbacks

        [UIAction("pause-button-clicked")]
        private void HandlePauseButtonClicked() {
            if (!_pauseController.IsPaused)
                _pauseController.Pause();
            else
                _pauseController.Resume();
        }

        [UIAction("settings-button-clicked")]
        private void HandleSettingsButtonClicked() {
            SettingsButtonClickedEvent?.Invoke();
        }

        private void HandleExitButtonClicked() {
            _finishController?.Exit();
        }

        private void HandleExitButtonSelectionStateChanged(SelectionState state) {
            _exitButtonIcon.Image.sprite = state switch {
                SelectionState.Pressed => _openedDoorSprite,
                SelectionState.Highlighted => _openedDoorSprite,
                _ => _closedDoorSprite
            };
        }

        private void HandlePauseStateChanged(bool pause) {
            _playButton.TargetGraphic.sprite = pause ? _playSprite : _pauseSprite;
        }

        #endregion
    }
}
