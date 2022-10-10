using BeatLeader.Models;
using BeatLeader.Utils;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class PlaybackController : MonoBehaviour, IReplayPauseController, IReplayExitController
    {
        #region Injection

        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly PauseController _pauseController;
        [Inject] private readonly AudioTimeSyncController _songTimeSyncController;
        [Inject] private readonly SaberManager _saberManager;
        [Inject] private readonly IGamePause _gamePause;

        [Inject] private readonly IVRPlatformHelper _vrPlatformHelper;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger;

        #endregion

        #region Info & Events

        public bool IsPaused => _gamePause.isPaused;

        public event Action<bool> PauseStateChangedEvent;

        #endregion

        #region Initialize & Dispose

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

        #endregion

        #region Pause & Resume

        public void Pause(bool notifyListeners = true, bool forcePause = false)
        {
            if (forcePause) SetPauseState(false);

            _gamePause.Pause();
            _saberManager.disableSabers = false;
            _songTimeSyncController.Pause();

            if (notifyListeners) InvokePauseEvent(true);
            _beatmapObjectManager.PauseAllBeatmapObjects(true);
            PauseStateChangedEvent?.Invoke(true);
        }

        public void Resume(bool notifyListeners = true, bool forceResume = false)
        {
            if (forceResume) SetPauseState(false);

            _gamePause.WillResume();
            _gamePause.Resume();
            _songTimeSyncController.Resume();

            if (notifyListeners) InvokePauseEvent(false);
            _beatmapObjectManager.PauseAllBeatmapObjects(false);
            PauseStateChangedEvent?.Invoke(false);
        }

        #endregion

        #region Exit

        public void Exit()
        {
            _pauseController.HandlePauseMenuManagerDidPressMenuButton();
        }

        #endregion

        #region Event Handlers

        private void HandleMenuButtonTriggered()
        {
            if (!IsPaused)
                Pause();
            else
                Resume();
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