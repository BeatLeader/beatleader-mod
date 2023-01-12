using BeatLeader.Models;
using BeatLeader.Utils;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayPauseController : MonoBehaviour, IReplayPauseController {
        #region Injection

        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager = null!;
        [Inject] private readonly PauseController _pauseController = null!;
        [Inject] private readonly AudioTimeSyncController _songTimeSyncController = null!;
        [Inject] private readonly PauseMenuManager _pauseMenuManager = null!;
        [Inject] private readonly SaberManager _saberManager = null!;
        [Inject] private readonly IGamePause _gamePause = null!;

        [Inject] private readonly IVRPlatformHelper _vrPlatformHelper = null!;
        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger = null!;
        [Inject] private readonly ILevelStartController _levelStartController = null!;

        #endregion

        #region Info & Events

        public bool LockUnpause { get; set; }
        public bool IsPaused => _gamePause.isPaused;

        public event Action<bool>? PauseStateChangedEvent;

        #endregion

        #region Setup

        private void Awake() {
            UnsubscribeStandardEvents();
            _vrPlatformHelper.hmdUnmountedEvent += HandleHMDUnmounted;
            _vrPlatformHelper.inputFocusWasCapturedEvent += HandleInputFocusWasLost;
            _pauseButtonTrigger.menuButtonTriggeredEvent += HandleMenuButtonTriggered;
        }

        private void OnDestroy() {
            _vrPlatformHelper.hmdUnmountedEvent -= HandleHMDUnmounted;
            _vrPlatformHelper.inputFocusWasCapturedEvent -= HandleInputFocusWasLost;
            _pauseButtonTrigger.menuButtonTriggeredEvent -= HandleMenuButtonTriggered;
            SubscribeStandardEvents();
        }

        private void UnsubscribeStandardEvents() {
            _vrPlatformHelper.inputFocusWasCapturedEvent -= _pauseController.HandleInputFocusWasCaptured;
            _vrPlatformHelper.hmdUnmountedEvent -= _pauseController.HandleHMDUnmounted;
            _pauseMenuManager.didFinishResumeAnimationEvent -= _pauseController.HandlePauseMenuManagerDidFinishResumeAnimation;
            _pauseMenuManager.didPressContinueButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressContinueButton;
            _pauseMenuManager.didPressRestartButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressRestartButton;
            _levelStartController.levelDidStartEvent -= _pauseController.HandleLevelDidStart;
            _levelStartController.levelWillStartIntroEvent -= _pauseController.HandleLevelWillStartIntro;
        }

        private void SubscribeStandardEvents() {
            _vrPlatformHelper.inputFocusWasCapturedEvent += _pauseController.HandleInputFocusWasCaptured;
            _vrPlatformHelper.hmdUnmountedEvent += _pauseController.HandleHMDUnmounted;
            _pauseMenuManager.didFinishResumeAnimationEvent += _pauseController.HandlePauseMenuManagerDidFinishResumeAnimation;
            _pauseMenuManager.didPressContinueButtonEvent += _pauseController.HandlePauseMenuManagerDidPressContinueButton;
            _pauseMenuManager.didPressRestartButtonEvent += _pauseController.HandlePauseMenuManagerDidPressRestartButton;
            _levelStartController.levelDidStartEvent += _pauseController.HandleLevelDidStart;
            _levelStartController.levelWillStartIntroEvent += _pauseController.HandleLevelWillStartIntro;
        }

        #endregion

        #region Pause & Resume

        public void Pause(bool notifyListeners = true, bool forcePause = false) {
            if (forcePause) SetPauseState(false);

            _gamePause.Pause();
            _saberManager.disableSabers = false;
            _songTimeSyncController.Pause();

            if (notifyListeners) InvokePauseEvent(true);
            _beatmapObjectManager.PauseAllBeatmapObjects(true);
            PauseStateChangedEvent?.Invoke(true);
        }

        public void Resume(bool notifyListeners = true, bool forceResume = false) {
            if (LockUnpause) return;
            if (forceResume) SetPauseState(false);

            _gamePause.WillResume();
            _gamePause.Resume();
            _songTimeSyncController.Resume();

            if (notifyListeners) InvokePauseEvent(false);
            _beatmapObjectManager.PauseAllBeatmapObjects(false);
            PauseStateChangedEvent?.Invoke(false);
        }

        #endregion

        #region Callbacks

        private void HandleMenuButtonTriggered() {
            if (!IsPaused)
                Pause();
            else
                Resume();
        }
        private void HandleHMDUnmounted() {
            Pause(true);
        }
        private void HandleInputFocusWasLost() {
            Pause(true);
        }

        #endregion

        #region Reflection

        private static readonly FieldInfo _didPauseEventInfo =
            typeof(PauseController).GetField("didPauseEvent", ReflectionUtils.DefaultFlags);
        private static readonly FieldInfo _didResumeEventInfo =
            typeof(PauseController).GetField("didResumeEvent", ReflectionUtils.DefaultFlags);
        private static readonly FieldInfo _pauseInfo =
            typeof(GamePause).GetField("_pause", ReflectionUtils.DefaultFlags);

        private void SetPauseState(bool pause) {
            _pauseInfo?.SetValue(_gamePause, pause);
        }
        private void InvokePauseEvent(bool pause) {
            ((Delegate?)(pause ? _didPauseEventInfo : _didResumeEventInfo)?.GetValue(_pauseController))?.DynamicInvoke();
        }

        #endregion
    }
}