using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackVRViewController : NotifiableSingleton<PlaybackVRViewController>
    {
        [Inject] protected readonly PlaybackController _playbackController;
        [Inject] protected readonly PlayerCameraController _playerCameraController;
        [Inject] protected readonly MultiplatformUIManager _multiplatformUIManager;

        [UIObject("timeline")] protected GameObject _timelineContainer;

        [UIValue("total-song-time")] protected int _totalSongTime;

        [UIValue("pause-button-text")] protected string pauseButtonText
        {
            get => _pauseButtonText;
            set
            {
                _pauseButtonText = value;
                NotifyPropertyChanged(nameof(pauseButtonText));
            }
        }
        [UIValue("combined-song-time")] protected string combinedSongTime
        {
            get => _combinedSongTime;
            set
            {
                _combinedSongTime = value;
                NotifyPropertyChanged(nameof(combinedSongTime));
            }
        }
        [UIValue("current-song-time")] protected int currentSongTime
        {
            get => _currentSongTime;
            set
            {
                _currentSongTime = value;
                NotifyPropertyChanged(nameof(currentSongTime));
            }
        }

        protected const string _viewPath = Plugin.ResourcesPath + ".BSML.PlaybackVRUI.bsml";

        protected string _pauseButtonText;
        protected string _combinedSongTime;
        protected int _currentSongTime;

        protected bool _initialized;
        protected bool _onPause;

        public void Start()
        {
            Debug.LogWarning("Installing VR replayer interface");
            _totalSongTime = (int)_playbackController.totalSongTime;
            _pauseButtonText = "Pause";

            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), _viewPath), _multiplatformUIManager.floatingScreen.gameObject, this);

            //(_timelineContainer.transform.Find("BSMLSlider") as RectTransform).anchorMin = new Vector2(0.35f, 0);

            _initialized = true;
        }
        public void Update()
        {
            if (_initialized)
            {
                currentSongTime = (int)_playbackController.currentSongTime;
                combinedSongTime = $"{currentSongTime}:{_totalSongTime}";
            }
        }

        [UIAction("menu-button-clicked")] protected void HandleMenuButtonClicked()
        {
            _playbackController.EscapeToMenu();
        }
        [UIAction("pause-button-clicked")] protected void HandlePauseButtonClicked()
        {
            _onPause = !_onPause;
            if (_onPause)
            {
                pauseButtonText = "Resume";
                _playbackController.Pause();
            }
            else
            {
                pauseButtonText = "Pause";
                _playbackController.Resume();
            }
        }
        [UIAction("pin-button-clicked")] protected void HandlePinButtonClicked()
        {
            if (_multiplatformUIManager.pinPose == MultiplatformUIManager.PinPose.Left)
                _multiplatformUIManager.PinToPose(MultiplatformUIManager.PinPose.Right);
            else if (_multiplatformUIManager.pinPose == MultiplatformUIManager.PinPose.Right)
                _multiplatformUIManager.PinToPose(MultiplatformUIManager.PinPose.Left);
        }
    }
}
