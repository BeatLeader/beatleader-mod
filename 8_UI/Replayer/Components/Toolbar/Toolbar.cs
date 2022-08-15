using System;
using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.Replays;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using HMUI;
using BeatLeader.Components.Settings;

namespace BeatLeader.Components
{
    internal class Toolbar : ReeUIComponentV2WithContainer
    {
        [Inject] private readonly PlaybackController _playbackController;

        [UIComponent("play-button")] private BetterButton _playButton;
        [UIComponent("container")] private RectTransform _container;

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

        public RectTransform Root => _container; 

        private Sprite _playSprite;
        private Sprite _pauseSprite;
        private string _combinedSongTime;
        private bool _onPause;

        protected override void OnInstantiate()
        {
            _playSprite = BSMLUtility.LoadSprite("#play-icon");
            _pauseSprite = BSMLUtility.LoadSprite("#pause-icon");
            _timeline = InstantiateInContainer<Timeline>(Container, transform);
            _settingsNavigator = InstantiateInContainer<SettingsController>(Container, transform);
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
        [UIAction("exit-button-clicked")] private void ExitButtonClicked()
        {
            _playbackController.EscapeToMenu();
        }
        [UIAction("pause-button-clicked")] private void PauseButtonClicked()
        {
            _playbackController.Pause(_onPause = !_onPause);
            _playButton.TargetGraphic.sprite = _onPause ? _playSprite : _pauseSprite;
        }
    }
}
