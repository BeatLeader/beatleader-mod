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
using BeatLeader.Replayer.Emulating;
using BeatLeader.Replayer.Movement;
using BeatLeader.Replayer.Managers;
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
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
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
            if (force)
            {
                _gamePause.GetType().GetField("_pause", BindingFlags.NonPublic
                        | BindingFlags.Instance).SetValue(_gamePause, !pause);
            }

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

            if (notify)
            {
                ((Delegate)_pauseController.GetType().GetField(pause ? "didPauseEvent" : "didResumeEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .GetValue(_pauseController))?.DynamicInvoke();
            }

            _beatmapObjectManager.PauseAllBeatmapObjects(pause);
            _beatmapEffectsController.PauseEffects(pause);
        }
        public void EscapeToMenu() => _pauseMenuManager.MenuButtonPressed();
    }
}