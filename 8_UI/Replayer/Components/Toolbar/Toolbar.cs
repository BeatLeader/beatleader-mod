using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components.Settings;
using BeatLeader.UI.BSML_Addons.Components;
using UnityEngine.UI;
using UnityEngine;
using HMUI;
using BeatLeader.Utils;
using BeatLeader.Models;
using static HMUI.NoTransitionsButton;

namespace BeatLeader.Components
{
    internal class Toolbar : EditableElement
    {
        #region UI Components

        [UIComponent("exit-button-background")] private readonly RectTransform _exitButtonBackground;
        [UIComponent("container")] private readonly RectTransform _container;

        [UIComponent("play-button")] private readonly BetterButton _playButton;
        [UIComponent("exit-button-icon")] private readonly BetterImage _exitButtonIcon;
        [UIComponent("settings-modal")] private readonly ModalView _settingsModal;

        [UIValue("settings-navigator")] private SettingsController _settingsNavigator;
        [UIValue("timeline")] private Timeline _timeline;

        private NoTransitionsButton _exitButton;

        #endregion

        #region FormattedSongTime

        [UIValue("combined-song-time")]
        public string FormattedSongTime
        {
            get => _formattedSongTime;
            private set
            {
                _formattedSongTime = value;
                NotifyPropertyChanged(nameof(FormattedSongTime));
            }
        }

        #endregion

        #region Editable

        protected override RectTransform ContainerRect => _container;
        public override bool Locked => true;

        #endregion

        #region Setup

        private readonly Sprite _playSprite = BSMLUtility.LoadSprite("#play-icon");
        private readonly Sprite _pauseSprite = BSMLUtility.LoadSprite("#pause-icon");
        private readonly Sprite _openedDoorSprite = BSMLUtility.LoadSprite("#opened-door-icon");
        private readonly Sprite _closedDoorSprite = BSMLUtility.LoadSprite("#closed-door-icon");
        private string _formattedSongTime;

        private IReplayPauseController _pauseController;
        private IReplayExitController _exitController;
        private IBeatmapTimeController _beatmapTimeController;
        private Replay _replay;

        public void Setup(
            Replay replay,
            IReplayPauseController pauseController,
            IReplayExitController exitController,
            IBeatmapTimeController beatmapTimeController)
        {
            if (_pauseController != null)
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;

            _replay = replay;
            _pauseController = pauseController;
            _exitController = exitController;
            _beatmapTimeController = beatmapTimeController;

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _timeline.Setup(_replay, _pauseController, _beatmapTimeController);
        }

        protected override void OnInstantiate()
        {
            _timeline = Instantiate<Timeline>(transform);
            _settingsNavigator = InstantiateInContainer<SettingsController>(Container, transform);
            _settingsNavigator.RootMenu = MenuWithContainer.InstantiateInContainer<SettingsRootMenu>(Container);
        }
        protected override void OnInitialize()
        {
            _exitButton = _exitButtonBackground.gameObject.AddComponent<NoTransitionsButton>();
            _exitButton.selectionStateDidChangeEvent += HandleExitButtonSelectionStateChanged;
            _exitButton.navigation = new Navigation() { mode = Navigation.Mode.None };
            _exitButton.onClick.AddListener(HandleExitButtonClicked);

            _settingsModal.blockerClickedEvent += _settingsNavigator.HandleSettingsWasClosed;
            _settingsNavigator.SettingsCloseRequestedEvent += HandleSettingsCloseRequested;
        }

        #endregion

        #region Update Song Time

        private void Update()
        {
            UpdateSongTime();
        }
        private void UpdateSongTime()
        {
            float time = _beatmapTimeController.SongTime;
            float totalTime = _beatmapTimeController.TotalSongTime;

            FormattedSongTime = FormatUtils.FormatSongTime(time, totalTime);
        }

        #endregion

        #region UI Event Handlers

        [UIAction("pause-button-clicked")]
        private void HandlePauseButtonClicked()
        {
            if (_pauseController == null) return;

            if (!_pauseController.IsPaused)
                _pauseController.Pause();
            else
                _pauseController.Resume();
        }

        private void HandleExitButtonClicked()
        {
            _exitController?.Exit();
        }

        #endregion

        #region Event Handlers

        private void HandleExitButtonSelectionStateChanged(SelectionState state)
        {
            _exitButtonIcon.Image.sprite = state switch
            {
                SelectionState.Pressed => _openedDoorSprite,
                SelectionState.Highlighted => _openedDoorSprite,
                _ => _closedDoorSprite
            };
        }
        private void HandleSettingsCloseRequested(bool animated)
        {
            _settingsModal.Hide(animated, _settingsNavigator.HandleSettingsWasClosed);
        }
        private void HandlePauseStateChanged(bool pause)
        {
            _playButton.TargetGraphic.sprite = pause ? _playSprite : _pauseSprite;
        }

        #endregion
    }
}
