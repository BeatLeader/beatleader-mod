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
        [Inject] private readonly InputManager _inputManager;
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly SoftLocksController _softLocksController;
        [Inject] private readonly ReplayerManualInstaller.InitData _initData;
        [Inject] private readonly StandardLevelGameplayManager _gameplayManager;
        [Inject] private readonly PauseController _pauseController;

        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger;
        [Inject] private readonly IVRPlatformHelper _vrPlatformHelper;
        [Inject] private readonly ILevelStartController _levelStartController;

        private void Start()
        {
            ApplyTweaks();
            UnsubscribeEvents();
        }
        private void ApplyTweaks()
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
        private void UnsubscribeEvents()
        {
            _pauseButtonTrigger.menuButtonTriggeredEvent -= _pauseController.HandleMenuButtonTriggered;
            _vrPlatformHelper.inputFocusWasCapturedEvent -= _pauseController.HandleInputFocusWasCaptured;
            _vrPlatformHelper.hmdUnmountedEvent -= _pauseController.HandleHMDUnmounted;
            _pauseMenuManager.didFinishResumeAnimationEvent -= _pauseController.HandlePauseMenuManagerDidFinishResumeAnimation;
            _pauseMenuManager.didPressContinueButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressContinueButton;
            _pauseMenuManager.didPressRestartButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressRestartButton;
            //_pauseMenuManager.didPressMenuButtonEvent -= _pauseController.HandlePauseMenuManagerDidPressMenuButton;
            _levelStartController.levelDidStartEvent -= _pauseController.HandleLevelDidStart;
            _levelStartController.levelWillStartIntroEvent -= _pauseController.HandleLevelWillStartIntro;
        }
    }
}
