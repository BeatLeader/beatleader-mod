using BeatLeader.Utils;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class PlaybackController : MonoBehaviour
    {
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly PauseController _pauseController;
        [Inject] private readonly AudioTimeSyncController _songTimeSyncController;
        [Inject] private readonly GameplayModifiers _modifiers;
        [Inject] private readonly SaberManager _saberManager;
        [Inject] private readonly IGamePause _gamePause;

        [Inject] private readonly IVRPlatformHelper _vrPlatformHelper;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger;

        public float CurrentSongTime => _songTimeSyncController.songTime;
        public float TotalSongTime => _songTimeSyncController.songEndTime;
        public float CurrentSongSpeedMultiplier => _songTimeSyncController.timeScale;
        public float SongSpeedMultiplier => _modifiers.songSpeedMul;
        public bool IsPaused => _gamePause.isPaused;

        public event Action<bool> PauseStateChangedEvent;

        private void Awake()
        {
            _didPauseEventInfo = typeof(PauseController).GetField("didPauseEvent", ReflectionUtils.DefaultFlags);
            _didResumeEventInfo = typeof(PauseController).GetField("didResumeEvent", ReflectionUtils.DefaultFlags);
            _pauseInfo = _gamePause.GetType().GetField("_pause", ReflectionUtils.DefaultFlags);

            _vrPlatformHelper.hmdUnmountedEvent += HandleHMDUnmounted;
            _vrPlatformHelper.inputFocusWasCapturedEvent += HandleInputFocusWasLost;
            _pauseButtonTrigger.menuButtonTriggeredEvent += HandleMenuButtonTriggered;
        }
        private void OnDestroy()
        {
            _vrPlatformHelper.hmdUnmountedEvent -= HandleHMDUnmounted;
            _vrPlatformHelper.inputFocusWasCapturedEvent -= HandleInputFocusWasLost;
            _pauseButtonTrigger.menuButtonTriggeredEvent -= HandleMenuButtonTriggered;
        }
        public void Pause(bool pause, bool notify = true, bool force = false)
        {
            if (force) SetPauseState(!pause);

            if (pause)
            {
                _gamePause.Pause();
                _saberManager.disableSabers = false;
                _songTimeSyncController.Pause();
            }
            else
            {
                _gamePause.WillResume();
                _gamePause.Resume();
                _songTimeSyncController.Resume();
            }

            if (notify) InvokePauseEvent(pause);
            _beatmapObjectManager.PauseAllBeatmapObjects(pause);
            PauseStateChangedEvent?.Invoke(pause);
        }
        public void EscapeToMenu() => _pauseMenuManager.MenuButtonPressed();

        #region Events

        private void HandleMenuButtonTriggered()
        {
            Pause(!IsPaused);
        }
        private void HandleHMDUnmounted()
        {
           Pause(true);
        }
        private void HandleInputFocusWasLost()
        {
            Pause(true);
        }

        #endregion

        #region Reflection

        private FieldInfo _didPauseEventInfo;
        private FieldInfo _didResumeEventInfo;
        private FieldInfo _pauseInfo;

        private void SetPauseState(bool pause)
        {
            _pauseInfo.SetValue(_gamePause, pause);
        }
        private void InvokePauseEvent(bool pause)
        {
            var info = pause ? _didPauseEventInfo : _didResumeEventInfo;
            ((Delegate)info.GetValue(_pauseController))?.DynamicInvoke();
        }

        #endregion
    }
}