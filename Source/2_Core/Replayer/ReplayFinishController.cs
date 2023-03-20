﻿using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using System;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayFinishController : MonoBehaviour, IReplayFinishController {
        [Inject] private readonly StandardLevelGameplayManager.InitData _gameplayManagerInitData = null!;
        [Inject] private readonly StandardLevelGameplayManager _gameplayManager = null!;
        [Inject] private readonly PauseMenuManager _pauseMenuManager = null!;
        [Inject] private readonly PauseController _pauseController = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly GameSongController _songController = null!;
        [Inject] private readonly IGameEnergyCounter _gameEnergyCounter = null!;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        public bool ExitAutomatically => _launchData.Settings.ExitReplayAutomatically;

        public event Action? ReplayWasLeftEvent;
        public event Action? ReplayWasFinishedEvent;

        private void Start() {
            this.LoadResources();
            _pauseMenuManager.didPressMenuButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressMenuButton;
            _pauseButtonTrigger.menuButtonTriggeredEvent -= _pauseController.HandleMenuButtonTriggered;
            _gameEnergyCounter.gameEnergyDidReach0Event -= _gameplayManager.HandleGameEnergyDidReach0;
            _songController.songDidFinishEvent -= _gameplayManager.HandleSongDidFinish;
            _gameEnergyCounter.gameEnergyDidReach0Event += HandleLevelFailed;
            _songController.songDidFinishEvent += HandleLevelFinished;
            _timeController.SongWasRewoundEvent += HandleSongRewound;
        }

        private void OnDestroy() {
            _timeController.SongWasRewoundEvent -= HandleSongRewound;
            _gameEnergyCounter.gameEnergyDidReach0Event -= HandleLevelFailed;
            _songController.songDidFinishEvent -= HandleLevelFinished;
            _pauseMenuManager.didPressMenuButtonEvent += _pauseController.HandlePauseMenuManagerDidPressMenuButton;
            _pauseButtonTrigger.menuButtonTriggeredEvent += _pauseController.HandleMenuButtonTriggered;
            _gameEnergyCounter.gameEnergyDidReach0Event += _gameplayManager.HandleGameEnergyDidReach0;
            _songController.songDidFinishEvent += _gameplayManager.HandleSongDidFinish;
        }

        public void Exit() {
            ReplayWasLeftEvent?.Invoke();
            _pauseController.HandlePauseMenuManagerDidPressMenuButton();
        }

        private void HandleLevelFinished() {
            ReplayWasFinishedEvent?.Invoke();
            if (ExitAutomatically) Exit();
        }

        private void HandleLevelFailed() {
            if (!_gameplayManagerInitData.failOn0Energy) return;
            HandleLevelFinished();
        }

        private void HandleSongRewound(float time) {
            _songController.SetField("_songDidFinish", false);
        }
    }
}
