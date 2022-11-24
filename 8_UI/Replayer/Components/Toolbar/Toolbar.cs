using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.UI.BSML_Addons.Components;
using UnityEngine.UI;
using UnityEngine;
using HMUI;
using BeatLeader.Models;
using static HMUI.NoTransitionsButton;
using System;

namespace BeatLeader.Components {
    internal class Toolbar : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("exit-button-background")] private readonly RectTransform _exitButtonBackground;
        [UIComponent("container")] private readonly RectTransform _container;

        [UIComponent("play-button")] private readonly BetterButton _playButton;
        [UIComponent("exit-button-icon")] private readonly BetterImage _exitButtonIcon;

        //[UIValue("settings-container")] private SettingsContainer _settingsContainer;
        [UIValue("timeline")] private Timeline _timeline;

        private NoTransitionsButton _exitButton;

        #endregion

        #region Events

        public event Action SettingsButtonClickedEvent;

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
        private string _formattedSongTime;

        private IReplayPauseController _pauseController;
        private IReplayExitController _exitController;
        private IBeatmapTimeController _beatmapTimeController;
        private Replay _replay;

        public void Setup(
            Replay replay,
            IReplayPauseController pauseController,
            IReplayExitController exitController,
            IBeatmapTimeController beatmapTimeController) {
            if (_pauseController != null)
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;

            _replay = replay;
            _pauseController = pauseController;
            _exitController = exitController;
            _beatmapTimeController = beatmapTimeController;

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _timeline.Setup(_replay, _pauseController, _beatmapTimeController);
        }

        protected override void OnInstantiate() {
            _timeline = Instantiate<Timeline>(transform);
            //_settingsContainer = Instantiate<SettingsContainer>(transform);
        }
        protected override void OnInitialize() {
            _exitButton = _exitButtonBackground.gameObject.AddComponent<NoTransitionsButton>();
            _exitButton.selectionStateDidChangeEvent += HandleExitButtonSelectionStateChanged;
            _exitButton.navigation = new Navigation() { mode = Navigation.Mode.None };
            _exitButton.onClick.AddListener(HandleExitButtonClicked);
        }

        #endregion

        #region UpdateSongTime

        private void Update() {
            UpdateSongTime();
        }
        private void UpdateSongTime() {
            float time = _beatmapTimeController.SongTime;
            float totalTime = _replay.info.failTime <= 0 ? 
                _beatmapTimeController.SongEndTime : _replay.info.failTime;

            FormattedSongTime = FormatUtils.FormatSongTime(time, totalTime);
        }

        #endregion

        #region UI Callbacks

        [UIAction("pause-button-clicked")]
        private void HandlePauseButtonClicked() {
            if (_pauseController == null) return;

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
            _exitController?.Exit();
        }

        #endregion

        #region Callbacks

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
