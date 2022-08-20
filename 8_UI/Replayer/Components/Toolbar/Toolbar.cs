using System;
using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components.Settings;
using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using HMUI;

namespace BeatLeader.Components
{
    internal class Toolbar : EditableElement
    {
        [Inject] private readonly PlaybackController _playbackController;

        [UIComponent("container")] private RectTransform _container;
        [UIComponent("play-button")] private BetterButton _playButton;
        [UIComponent("exit-button-background")] private RectTransform _exitButtonBackground;
        [UIComponent("exit-button-icon")] private BetterImage _exitButtonIcon;
        [UIComponent("settings-modal")] private ModalView _settingsModal;

        [UIValue("combined-song-time")] private string combinedSongTime
        {
            get => _combinedSongTime;
            set
            {
                _combinedSongTime = value;
                NotifyPropertyChanged(nameof(combinedSongTime));
            }
        }
        [UIValue("timeline")] private Timeline _timeline;
        [UIValue("settings-navigator")] private SettingsController _settingsNavigator;

        protected override RectTransform ContainerRect => _container;
        public override bool Locked => true;

        private Sprite _playSprite;
        private Sprite _pauseSprite;
        private Sprite _openedDoorSprite;
        private Sprite _closedDoorSprite;
        private string _combinedSongTime;
        private bool _onPause;

        protected override void OnInstantiate()
        {
            _playSprite = BSMLUtility.LoadSprite("#play-icon");
            _pauseSprite = BSMLUtility.LoadSprite("#pause-icon");
            _openedDoorSprite = BSMLUtility.LoadSprite("#opened-door-icon");
            _closedDoorSprite = BSMLUtility.LoadSprite("#closed-door-icon");
            _timeline = InstantiateInContainer<Timeline>(Container, transform);
            _settingsNavigator = InstantiateInContainer<SettingsController>(Container, transform);
            _settingsNavigator.RootMenu = MenuWithContainer.InstantiateInContainer<SettingsRootMenu>(Container);
        }
        protected override void OnInitialize()
        {
            var button = _exitButtonBackground.gameObject.AddComponent<NoTransitionsButton>();
            button.selectionStateDidChangeEvent += ExitButtonSelectionStateChanged;
            button.onClick.AddListener(_playbackController.EscapeToMenu);
            button.navigation = new Navigation() { mode = Navigation.Mode.None };
            _settingsModal.blockerClickedEvent += _settingsNavigator.NotifySettingsClosed;
            _settingsNavigator.OnSettingsCloseRequested += x => _settingsModal.Hide(x, () => _settingsNavigator.NotifySettingsClosed());
        }
        private void Update()
        {
            float time = _playbackController.CurrentSongTime;
            float totalTime = _playbackController.TotalSongTime;

            float minutes = Mathf.FloorToInt(time / 60);
            float seconds = Mathf.FloorToInt(time - (minutes * 60));
            float totalMinutes = Mathf.FloorToInt(totalTime / 60);
            float totalSeconds = Mathf.FloorToInt(totalTime - (totalMinutes * 60));

            string combinedTotalTime = $"{totalMinutes}.{(totalSeconds < 10 ? $"0{totalSeconds}" : totalSeconds)}";
            combinedSongTime = $"{minutes}.{(seconds < 10 ? $"0{seconds}" : seconds)}/{combinedTotalTime}";
        }

        private void ExitButtonSelectionStateChanged(NoTransitionsButton.SelectionState state)
        {
            if (state == NoTransitionsButton.SelectionState.Highlighted)
            {
                _exitButtonIcon.Image.sprite = _openedDoorSprite;
            }
            if (state == NoTransitionsButton.SelectionState.Normal)
            {
                _exitButtonIcon.Image.sprite = _closedDoorSprite;
            }
        }
        [UIAction("pause-button-clicked")] private void PauseButtonClicked()
        {
            _playbackController.Pause(_onPause = !_onPause);
            _playButton.TargetGraphic.sprite = _onPause ? _playSprite : _pauseSprite;
        }
    }
}
