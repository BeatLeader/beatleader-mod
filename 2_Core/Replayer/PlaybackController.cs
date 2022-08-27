using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Replayer.Movement;
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
        [Inject] private readonly BeatmapVisualsController _beatmapEffectsController;

        public float CurrentSongTime => _songTimeSyncController.songTime;
        public float TotalSongTime => _songTimeSyncController.songEndTime;
        public float CurrentSongSpeedMultiplier => _songTimeSyncController.timeScale;
        public float SongSpeedMultiplier => _modifiers.songSpeedMul;
        public bool IsPaused => _gamePause.isPaused;

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
        }
        public void EscapeToMenu() => _pauseMenuManager.MenuButtonPressed();

        #region Reflection

        private FieldInfo _didPauseEventInfo;
        private FieldInfo _didResumeEventInfo;
        private FieldInfo _pauseInfo;

        private void Awake()
        {
            _didPauseEventInfo = _pauseController.GetType().GetField("didPauseEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _didResumeEventInfo = _pauseController.GetType().GetField("didResumeEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _pauseInfo = _gamePause.GetType().GetField("_pause", BindingFlags.NonPublic | BindingFlags.Instance);
        }
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