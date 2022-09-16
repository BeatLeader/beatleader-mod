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
        [Inject] private readonly BeatmapVisualsController _beatmapEffectsController;
        [Inject] private readonly IGamePause _gamePause;
        [Inject] private readonly IVRPlatformHelper _vrPlatformHelper;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger;

        public float CurrentSongTime => _songTimeSyncController.songTime;
        public float TotalSongTime => _songTimeSyncController.songEndTime;
        public float CurrentSongSpeedMultiplier => _songTimeSyncController.timeScale;
        public float SongSpeedMultiplier => _modifiers.songSpeedMul;
        public bool IsPaused => _gamePause.isPaused;

        public event Action<bool> OnPauseStateChanged;

        public void Pause(bool pause, bool notify = true, bool force = false)
        {
            if (force) SetPauseState(!pause);

            if (pause)
            {
                _gamePause.Pause();
                _saberManager.disableSabers = false;
            }
            else
            {
                _gamePause.WillResume();
                _gamePause.Resume();
            }

            if (notify) InvokePauseEvent(pause);
            _beatmapObjectManager.PauseAllBeatmapObjects(pause);
            _beatmapEffectsController.PauseEffects(pause);
            OnPauseStateChanged?.Invoke(pause);
        }
        public void EscapeToMenu() => _pauseMenuManager.MenuButtonPressed();

        private void Start()
        {
            _didPauseEventInfo = _pauseController.GetType().GetField("didPauseEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _didResumeEventInfo = _pauseController.GetType().GetField("didResumeEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _pauseInfo = _gamePause.GetType().GetField("_pause", BindingFlags.NonPublic | BindingFlags.Instance);

            _vrPlatformHelper.hmdUnmountedEvent += NotifyPauseRequired;
            _vrPlatformHelper.inputFocusWasCapturedEvent += NotifyPauseRequired;
            _pauseButtonTrigger.menuButtonTriggeredEvent += NotifyPauseSwitchRequired;
        }
        private void OnDestroy()
        {
            if (_vrPlatformHelper != null)
            {
                _vrPlatformHelper.hmdUnmountedEvent -= NotifyPauseRequired;
                _vrPlatformHelper.inputFocusWasCapturedEvent -= NotifyPauseRequired;
            }
            if (_pauseButtonTrigger != null)
            {
                _pauseButtonTrigger.menuButtonTriggeredEvent -= NotifyPauseSwitchRequired;
            }
        }
        private void NotifyPauseSwitchRequired()
        {
            Pause(!IsPaused);
        }
        private void NotifyPauseRequired()
        {
            Pause(true);
        }

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