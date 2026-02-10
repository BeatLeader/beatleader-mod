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
            pauseControllerDidPause += HandlePausedFromController;
            _pauseMenuManager.didPressContinueButtonEvent += HandleResumeFromPauseManager;
        }

        private void OnDestroy() {
            _vrPlatformHelper.hmdUnmountedEvent -= HandleHMDUnmounted;
            _vrPlatformHelper.inputFocusWasCapturedEvent -= HandleInputFocusWasLost;
            _pauseButtonTrigger.menuButtonTriggeredEvent -= HandleMenuButtonTriggered;
            pauseControllerDidPause -= HandlePausedFromController;
            _pauseMenuManager.didPressContinueButtonEvent -= HandleResumeFromPauseManager;

            _pauseControllerPausePatch.Dispose();
        }

        private void UnsubscribeStandardEvents() {
            _vrPlatformHelper.inputFocusWasCapturedEvent -= _pauseController.HandleFocusWasCaptured;
            _vrPlatformHelper.hmdUnmountedEvent -= _pauseController.HandleHMDUnmounted;
            _pauseMenuManager.didFinishResumeAnimationEvent -= _pauseController.HandlePauseMenuManagerDidFinishResumeAnimation;
            _pauseMenuManager.didPressContinueButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressContinueButton;
            _pauseMenuManager.didPressRestartButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressRestartButton;
            _levelStartController.levelDidStartEvent -= _pauseController.HandleLevelDidStart;
        }

        #endregion

        #region Pause & Resume

        public void Pause(bool notifyListeners = true, bool forcePause = false) {
            if (forcePause) SetPauseState(true);

            _gamePause.Pause();
            _pauseController._paused = PauseController.PauseState.Paused;
            _saberManager.disableSabers = false;
            _songTimeSyncController.Pause();

            if (notifyListeners) InvokePauseEvent(true);
            _beatmapObjectManager.PauseAllBeatmapObjects(true);
            PauseStateChangedEvent?.Invoke(true);
        }

        public void Resume(bool notifyListeners = true, bool forceResume = false) {
            if (LockUnpause) return;
            if (forceResume) SetPauseState(false);

            _pauseController._paused = PauseController.PauseState.Resuming;
            _gamePause.WillResume();
            _pauseController._paused = PauseController.PauseState.Playing;
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
        private void HandlePausedFromController() {
            Pause();
        }
        private void HandleResumeFromPauseManager() {
            Resume();
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

        private static readonly HarmonyPatchDescriptor pauseControllerPausePatchDescriptor = new(
            typeof(PauseController).GetMethod(
                nameof(PauseController.Pause),
                ReflectionUtils.DefaultFlags
            )!,
            prefix:
            typeof(ReplayPauseController).GetMethod(
                nameof(PauseControllerPausePatchedMethod),
                ReflectionUtils.StaticFlags
            )
        );
        private static Action? pauseControllerDidPause;

        private readonly HarmonyAutoPatch _pauseControllerPausePatch = new(pauseControllerPausePatchDescriptor);
        private static bool PauseControllerPausePatchedMethod() {
            pauseControllerDidPause?.Invoke();

            return false;
        }

        #endregion
    }
}