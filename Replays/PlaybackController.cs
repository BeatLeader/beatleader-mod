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
using BeatLeader.Replays.MapEmitating;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlaybackController : MonoBehaviour
    {
        [Inject] protected readonly MenuSabersManager _menuSabersManager;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;
        [Inject] protected readonly IGamePause _gamePause;
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly PauseController _pauseController;
        [Inject] protected readonly AudioTimeSyncController _songTimeSyncController;
        [Inject] protected readonly Replayer _replayer; 
        [Inject] protected readonly SaberManager _saberManager;
        [Inject] protected readonly SimpleTimeController _simpleTimeController;

        public float currentSongTime => _songTimeSyncController.songTime;
        public float totalSongTime => _songTimeSyncController.songEndTime;

        public void Start()
        {
            _menuSabersManager.ShowMenuControllers();
        }
        public void Pause()
        {
            _gamePause.Pause(); 
            _beatmapObjectManager.PauseAllBeatmapObjects(true);
            _saberManager.disableSabers = false;
        }
        public void Resume()
        {
            _gamePause.Resume();
            _beatmapObjectManager.PauseAllBeatmapObjects(false);
        }
        public void EscapeToMenu()
        {
            _pauseMenuManager.MenuButtonPressed();
        }
        public void SetTimeScale(float multiplier)
        {
            _simpleTimeController.SetTimeScale(multiplier);
        }
        public void ToTime(float time)
        {
            _simpleTimeController.ToTime(time);
        }
    }
}
