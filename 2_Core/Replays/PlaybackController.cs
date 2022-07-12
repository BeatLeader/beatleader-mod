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
using BeatLeader.Replays.Emulating;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackController : MonoBehaviour
    {
        [Inject] protected readonly VRControllersManager _vrControllersManager;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly PauseController _pauseController;
        [Inject] protected readonly AudioTimeSyncController _songTimeSyncController;
        [Inject] protected readonly VRControllersMovementEmulator _replayer;
        [Inject] protected readonly BeatmapTimeController _beatmapTimeController;
        [Inject] protected readonly GameplayModifiers _modifiers;

        [Inject] protected readonly SaberManager _saberManager;
        [Inject] protected readonly IGamePause _gamePause;
        [Inject] protected readonly BeatmapVisualsController _beatmapEffectsController;

        public float currentSongTime => _songTimeSyncController.songTime;
        public float totalSongTime => _songTimeSyncController.songEndTime;
        public float currentSongSpeedMultiplier => _songTimeSyncController.timeScale;
        public float songSpeedMultiplier => _modifiers.songSpeedMul;

        public void Start()
        {
            _vrControllersManager.ShowMenuControllers();
        }
        public void Pause(bool pause)
        {
            if (pause)
            {
                _gamePause.Pause();
                _saberManager.disableSabers = false;
                ((Delegate)_pauseController.GetType().GetField("didPauseEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_pauseController))?.DynamicInvoke();
            }
            else
            {
                _gamePause.Resume();
                ((Delegate)_pauseController.GetType().GetField("didResumeEvent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_pauseController))?.DynamicInvoke();
            }
            _beatmapObjectManager.PauseAllBeatmapObjects(pause);
            _beatmapEffectsController.PauseEffects(pause);
        }
        public void Rewind(float time)
        {
            _beatmapTimeController.Rewind(time);
        }
        public void SetSpeedMul(float multiplier)
        {
            _beatmapTimeController.SetTimeScale(multiplier);
        }
        public void EscapeToMenu()
        {
            _pauseMenuManager.MenuButtonPressed();
        }
    }
}
