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

        [Inject] protected readonly GameEnergyCounter _gameEnergyCounter;
        [Inject] protected readonly PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction;
        [Inject] protected readonly BeatmapObjectExecutionRatingsRecorder _beatmapObjectExecutionRatingsRecorder;
        [Inject] protected readonly SongController _songController;
        [Inject] protected readonly AudioListenerController _audioListenerController;
        [Inject] protected readonly IScoreController _scoreController;
        [Inject] protected readonly IGamePause _gamePause;

        public float currentSongTime => _songTimeSyncController.songTime;
        public float totalSongTime => _songTimeSyncController.songEndTime;

        public void Start()
        {
            _vrControllersManager.ShowMenuControllers();
        }
        public void Pause()
        {
            _gameEnergyCounter.enabled = false;
            _playerHeadAndObstacleInteraction.enabled = false;
            _scoreController.SetEnabled(false);
            _beatmapObjectExecutionRatingsRecorder.enabled = false;
            _audioListenerController.Pause();
            _songController.PauseSong();
            _beatmapObjectManager.PauseAllBeatmapObjects(true);
        }
        public void Resume()
        {
            _gameEnergyCounter.enabled = true;
            _playerHeadAndObstacleInteraction.enabled = true;
            _scoreController.SetEnabled(true);
            _beatmapObjectExecutionRatingsRecorder.enabled = true;
            _audioListenerController.Resume();
            _songController.ResumeSong();
            _beatmapObjectManager.PauseAllBeatmapObjects(false);
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
