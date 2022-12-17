using BeatLeader.Models;
using BeatLeader.Utils;
using System;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayFinishController : MonoBehaviour, IReplayFinishController {
        [Inject] private readonly StandardLevelGameplayManager _gameplayManager;
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly PauseController _pauseController;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger;
        [FirstResource] private readonly StandardLevelFinishedController _finishController;

        public bool ExitAutomatically { get; private set; }

        public event Action ReplayWasExitedEvent;
        public event Action ReplayWasFinishedEvent;

        private void Awake() {
            this.LoadResources();
            ExitAutomatically = _launchData.Settings.ExitReplayAutomatically;

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
            ReplayWasExitedEvent?.Invoke();
            _pauseController.HandlePauseMenuManagerDidPressMenuButton();
        }

        private void HandleLevelFinished() {
            ReplayWasFinishedEvent?.Invoke();
            if (ExitAutomatically) Exit();
        }
    }
}
