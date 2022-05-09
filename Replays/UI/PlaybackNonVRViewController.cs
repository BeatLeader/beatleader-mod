using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using BeatLeader.Models;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackNonVRViewController : NotifiableSingleton<PlaybackNonVRViewController>
    {
        [Inject] protected readonly PlaybackController _playbackController;
        [Inject] protected readonly PlayerCameraController _playerCameraController;
        [Inject] protected readonly MultiplatformUIManager _multiplatformUIManager;
        [Inject] protected readonly PauseMenuManager.InitData _pauseMenuInitData;
        [Inject] protected readonly Replay _replay;

        [UIObject("timeline")] protected GameObject _timelineContainer;
        [UIObject("song-preview-image")] protected GameObject _songPreviewImage;

        [UIValue("total-song-time")] protected int _totalSongTime;
        [UIValue("song-name")] protected string _songName;
        [UIValue("song-author")] protected string _songAuthor;
        [UIValue("player-name")] protected string _playerName;
        [UIValue("timestamp")] protected string _timestamp;

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
        [UIValue("time-scale")] protected int timeScaleValue
        {
            get => _timeScaleValue;
            set
            {
                _timeScaleValue = value;
                Debug.LogWarning($"scaling to {value * 0.01f}");
                _playbackController.SetTimeScale(value * 0.01f);
            }
        }
        [UIValue("current-song-time")] protected int currentSongTime
        {
            get => _currentSongTime;
            set
            {
                _currentSongTime = value;
                _playbackController.ToTime(value);
                Debug.LogWarning($"moving to {value}");
                NotifyPropertyChanged(nameof(currentSongTime));
            }
        }
        [UIValue("override-camera")] protected bool overrideCamera
        {
            get => _overrideCamera;
            set
            {
                _overrideCamera = value;
                _playerCameraController.SetEnabled(_overrideCamera);
            }
        }
        [UIValue("camera-fov")] protected int fieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;
                _playerCameraController.SetPlayerViewFOV(_fieldOfView);
            }
        }

        protected const string _viewPath = Plugin.ResourcesPath + ".BSML.PlaybackNonVRUI.bsml";

        protected string _pauseButtonText;
        protected string _combinedSongTime;
        protected int _timeScaleValue;
        protected int _currentSongTime;
        protected int _fieldOfView;
        protected bool _overrideCamera;

        protected bool _initialized;
        protected bool _onPause;

        protected IPreviewBeatmapLevel _previewBeatmapLevel => _pauseMenuInitData.previewBeatmapLevel;

        public void Start()
        {
            _totalSongTime = (int)_playbackController.totalSongTime;
            _pauseButtonText = "Pause";
            _timeScaleValue = 100;
            _overrideCamera = _playerCameraController.overrideCamera;
            _fieldOfView = _playerCameraController.fieldOfView;
            _songName = _previewBeatmapLevel.songName;
            _songAuthor = _previewBeatmapLevel.songAuthorName;
            _playerName = _replay.info.playerName;
            _timestamp = _replay.info.timestamp;

            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), _viewPath), _multiplatformUIManager.floatingScreen.gameObject, this);

            //(_timelineContainer.transform.Find("BSMLSlider") as RectTransform).anchorMin = new Vector2(0.35f, 0);
            Task.Run(LoadImage);
            _initialized = true;
        }
        public void Update()
        {
            if (_initialized)
            {
                //currentSongTime = (int)_playbackController.currentSongTime;
                combinedSongTime = $"{currentSongTime}:{_totalSongTime}";
            }
        }
        protected async void LoadImage()
        {
            _songPreviewImage.GetComponentInChildren<ImageView>().sprite = await _previewBeatmapLevel.GetCoverImageAsync(new CancellationTokenSource().Token);
        }

        [UIAction("menu-button-clicked")] protected void HandleMenuButtonClicked()
        {
             _playbackController.EscapeToMenu();
            //Debug.LogWarning("Seeking to 10sec");
            //_playbackController.ToTime(10f);
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
    }
}
