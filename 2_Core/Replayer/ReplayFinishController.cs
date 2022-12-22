using BeatLeader.Models;
using BeatLeader.Utils;
using System;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayFinishController : MonoBehaviour, IReplayFinishController {
        [Inject] private readonly StandardLevelGameplayManager _gameplayManager = null!;
        [Inject] private readonly PauseMenuManager _pauseMenuManager = null!;
        [Inject] private readonly PauseController _pauseController = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger = null!;

        [FirstResource] 
        private readonly StandardLevelFinishedController _finishController = null!;

        public bool ExitAutomatically => _launchData?.Settings.ExitReplayAutomatically ?? true;

        public event Action? ReplayWasLeftEvent;
        public event Action? ReplayWasFinishedEvent;

        private void Start() {
            this.LoadResources();
            _gameplayManager.levelFinishedEvent -= _finishController.HandleLevelFinished;
            _pauseMenuManager.didPressMenuButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressMenuButton;
            _pauseButtonTrigger.menuButtonTriggeredEvent -= _pauseController.HandleMenuButtonTriggered;
            _gameplayManager.levelFinishedEvent += HandleLevelFinished;
        }

        private void OnDestroy() {
            _gameplayManager.levelFinishedEvent -= HandleLevelFinished;
            _gameplayManager.levelFinishedEvent += _finishController.HandleLevelFinished;
            _pauseMenuManager.didPressMenuButtonEvent += _pauseController.HandlePauseMenuManagerDidPressMenuButton;
            _pauseButtonTrigger.menuButtonTriggeredEvent += _pauseController.HandleMenuButtonTriggered;
        }

        public void Exit() {
            ReplayWasLeftEvent?.Invoke();
            _pauseController.HandlePauseMenuManagerDidPressMenuButton();
        }

        private void HandleLevelFinished() {
            ReplayWasFinishedEvent?.Invoke();
            if (ExitAutomatically) Exit();
        }
    }
}
