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
        [Inject] protected readonly Replayer _replayer;
        [Inject] protected readonly SimpleTimeController _simpleTimeController;

        [Inject] protected readonly SaberManager _saberManager;
        [Inject] protected readonly IScoreController _scoreController;
        [Inject] protected readonly IGamePause _gamePause;

        public float currentSongTime => _songTimeSyncController.songTime;
        public float totalSongTime => _songTimeSyncController.songEndTime;

        public void Awake()
        {
            _vrControllersManager.ShowMenuControllers();
        }
        public void Pause()
        {
            _gamePause.Pause();
            _saberManager.disableSabers = false;
            _beatmapObjectManager.PauseAllBeatmapObjects(true);
            ((Delegate)_pauseController.GetType().GetField("didPauseEvent", 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_pauseController))?.DynamicInvoke();
        }
        public void Resume()
        {
            _gamePause.Resume();
            _beatmapObjectManager.PauseAllBeatmapObjects(false);
            ((Delegate)_pauseController.GetType().GetField("didResumeEvent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_pauseController))?.DynamicInvoke();
        }
        public void Rewind(float time)
        {
            _simpleTimeController.Rewind(time);
        }
        public void SetTimeScale(float multiplier)
        {
            _simpleTimeController.SetTimeScale(multiplier);
        }
        public void EscapeToMenu()
        {
            _pauseMenuManager.MenuButtonPressed();
        }
    }
}
