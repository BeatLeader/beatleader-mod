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
        [Inject] private readonly VRControllersManager _vrControllersManager;
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

        private void Start()
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
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_pauseController))?.DynamicInvoke();
            }
            else
            {
                _gamePause.Resume();
                ((Delegate)_pauseController.GetType().GetField("didResumeEvent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_pauseController))?.DynamicInvoke();
            }

            _beatmapObjectManager.PauseAllBeatmapObjects(pause);
            _beatmapEffectsController.PauseEffects(pause);
        }
        public void Rewind(float time) => _beatmapTimeController.Rewind(time);
        public void SetSpeedMul(float multiplier) => _beatmapTimeController.SetSpeedMultiplier(multiplier);
        public void EscapeToMenu() => _pauseMenuManager.MenuButtonPressed();
    }
}