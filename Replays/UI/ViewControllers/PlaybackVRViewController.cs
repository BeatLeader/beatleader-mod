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

namespace BeatLeader.Replays.UI
{
    public class PlaybackVRViewController : NotifiableSingleton<PlaybackVRViewController>, IPlaybackViewController
    {
        [Inject] protected readonly PlaybackController _playbackController;
        [Inject] protected readonly ReplayerCameraController _cameraController;
        [Inject] protected readonly VRControllersManager _vrControllersManager;

        [UIValue("total-song-time")] protected int _totalSongTime;
        [UIValue("camera-view-values")] protected List<object> _cameraViewValues;

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
        protected readonly Vector3 _defaultLeftPinPosePos = new Vector3(0.35f, 0, 0);
        protected readonly Vector3 _defaultRightPinPosePos = new Vector3(-0.75f, 0, 0);

        protected FloatingScreen _floatingScreen;
        protected string _cameraView;
        protected string _pauseButtonText;
        protected string _combinedSongTime;
        protected int _currentSongTime;

        protected bool _initialized;
        protected bool _onPause;

        public FloatingScreen floatingScreen => _floatingScreen;
        public GameObject root => gameObject;

        public void Init()
        {
            _totalSongTime = (int)_playbackController.totalSongTime;
            _pauseButtonText = "Pause";
            _cameraViewValues = new List<object>(_cameraController.poseProviders.Select(x => x.name));

            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(60, 60), false, new Vector3(), Quaternion.identity);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), _viewPath), _floatingScreen.gameObject, this);
            _floatingScreen.transform.localPosition = _defaultLeftPinPosePos;
            _floatingScreen.transform.localEulerAngles = new Vector3(90, 0, 0);
            _floatingScreen.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

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
        public void Enable()
        {
            _floatingScreen.gameObject.SetActive(true);
        }
        public void Disable()
        {
            _floatingScreen.gameObject.SetActive(false);
        }

        [UIAction("menu-button-clicked")] protected void HandleMenuButtonClicked()
        {
            _playbackController.EscapeToMenu();
        }
        [UIAction("pause-button-clicked")] protected void HandlePauseButtonClicked()
        {
            _onPause = !_onPause;
            _playbackController.Pause(_onPause);
        }
        [UIAction("pin-button-clicked")] protected void HandlePinButtonClicked()
        {

        }
    }
}
