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
using BeatLeader.Replays.Emulating;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using BeatLeader.Models;
using VRUIControls;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.UI
{
    public class PlaybackNonVRViewController : NotifiableSingleton<PlaybackNonVRViewController>, IPlaybackViewController
    {
        [Inject] protected readonly PlaybackController _playbackController;
        [Inject] protected readonly ReplayerCameraController _cameraController;
        [Inject] protected readonly SimpleTimeController _simpleTimeController;
        [Inject] protected readonly PauseMenuManager.InitData _pauseMenuInitData;
        [Inject] protected readonly UI2DManager _ui2DManager;
        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly Replay _replay;

        [UIObject("timeline")] protected GameObject _timelineContainer;
        [UIObject("song-preview-image")] protected GameObject _songPreviewImage;

        [UIValue("camera-view-values")] protected List<object> _cameraViewValues;
        [UIValue("total-song-time")] protected int _totalSongTime;
        [UIValue("song-name")] protected string _songName;
        [UIValue("song-author")] protected string _songAuthor;
        [UIValue("player-name")] protected string _playerName;
        [UIValue("timestamp")] protected string _timestamp;

        [UIValue("camera-view")] protected string cameraView
        {
            get => _cameraView;
            set
            {
                _cameraView = value;
                Debug.LogWarning(value);
                _cameraController.SetCameraPose(value);
            }
        }
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
                _playbackController.Rewind(value);
                Debug.LogWarning($"moving to {value}");
                NotifyPropertyChanged(nameof(currentSongTime));
            }
        }
        [UIValue("camera-fov")] protected int fieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;
                _cameraController.SetCameraFOV(value);
            }
        }

        protected const string _viewPath = Plugin.ResourcesPath + ".BSML.PlaybackNonVRUI.bsml";

        protected FloatingScreen _floatingScreen;
        protected string _cameraView;
        protected string _pauseButtonText;
        protected string _combinedSongTime;
        protected int _timeScaleValue;
        protected int _currentSongTime;
        protected int _fieldOfView;

        protected bool _initialized;
        protected bool _onPause;

        protected IPreviewBeatmapLevel _previewBeatmapLevel => _pauseMenuInitData.previewBeatmapLevel;

        public FloatingScreen floatingScreen => _floatingScreen;
        public GameObject root => gameObject;

        public void Init()
        {
            _totalSongTime = (int)_playbackController.totalSongTime;
            _pauseButtonText = "Pause";
            _timeScaleValue = 100;
            _fieldOfView = _cameraController.fieldOfView;
            _songName = _previewBeatmapLevel.songName;
            _songAuthor = _previewBeatmapLevel.songAuthorName;
            _playerName = "Replay by: " + _replay.info.playerName;
            _timestamp = "Timestamp: " + _replay.info.timestamp;
            _cameraViewValues = new List<object>(_cameraController.poseProviders.Select(x => x.name));

            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(200, 200), false, new UnityEngine.Vector3(-1.93f, -0.67f), UnityEngine.Quaternion.identity);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), _viewPath), _floatingScreen.gameObject, this);
            _floatingScreen.transform.localScale = new Vector2(0.7f, 0.7f);

            Task.Run(LoadImage);
            _initialized = true;
        }
        public void Update()
        {
            if (_initialized)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    //_inputManager.SwitchInputSystem();
                    InputManager.SwitchCursor();
                }
                //currentSongTime = (int)_playbackController.currentSongTime;
                combinedSongTime = $"{currentSongTime}:{_totalSongTime}";
            }
        }
        public void Enable()
        {
            _floatingScreen.gameObject.SetActive(true);
        }
        public void Disable()
        {
            _floatingScreen.gameObject.SetActive(false);
        }
        protected async void LoadImage()
        {
            _songPreviewImage.GetComponentInChildren<ImageView>().sprite = await _previewBeatmapLevel.GetCoverImageAsync(new CancellationTokenSource().Token);
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
    }
}
