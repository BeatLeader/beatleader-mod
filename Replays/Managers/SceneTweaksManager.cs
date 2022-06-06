using System;
using System.Linq;
using System.Reflection;
using SiraUtil.Tools.FPFC;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class SceneTweaksManager : MonoBehaviour
    {
        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;
        [Inject] protected readonly SoftLocksController _softLocksController;
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly StandardLevelGameplayManager _gameplayManager;
        [Inject] protected readonly PauseController _pauseController;

        [Inject] protected readonly IMenuButtonTrigger _pauseButtonTrigger;
        [Inject] protected readonly IVRPlatformHelper _vrPlatformHelper;
        [Inject] protected readonly ILevelStartController _levelStartController;

        public void Start()
        {
            ApplyTweaks();
            UnsubscribeEvents();
        }
        public void ApplyTweaks()
        {
            _softLocksController.InstallLock(_gameplayManager, SoftLocksController.LockMode.WhenRequired);
            _softLocksController.InstallLock(_pauseMenuManager, SoftLocksController.LockMode.WhenRequired);
            if (_initData.noteSyncMode)
            {
                _softLocksController.InstallLock(Resources.FindObjectsOfTypeAll<CuttingManager>().First(), SoftLocksController.LockMode.WhenRequired);
            }
            if (_inputManager.isInFPFC)
            {
                Resources.FindObjectsOfTypeAll<VRLaserPointer>().First().gameObject.SetActive(false);
                Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().First().gameObject.SetActive(false);
            }
            else
            {
                Resources.FindObjectsOfTypeAll<VRLaserPointer>().First().gameObject.SetActive(true);
                Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().First().gameObject.SetActive(true);
            }
        }
        public void UnsubscribeEvents()
        {
            _pauseButtonTrigger.menuButtonTriggeredEvent -= _pauseController.HandleMenuButtonTriggered;
            _vrPlatformHelper.inputFocusWasCapturedEvent -= _pauseController.HandleInputFocusWasCaptured;
            _vrPlatformHelper.hmdUnmountedEvent -= _pauseController.HandleHMDUnmounted;
            _pauseMenuManager.didFinishResumeAnimationEvent -= _pauseController.HandlePauseMenuManagerDidFinishResumeAnimation;
            _pauseMenuManager.didPressContinueButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressContinueButton;
            _pauseMenuManager.didPressRestartButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressRestartButton;
            _pauseMenuManager.didPressMenuButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressMenuButton;
            _levelStartController.levelDidStartEvent -= _pauseController.HandleLevelDidStart;
            _levelStartController.levelWillStartIntroEvent -= _pauseController.HandleLevelWillStartIntro;
        }
    }
}
