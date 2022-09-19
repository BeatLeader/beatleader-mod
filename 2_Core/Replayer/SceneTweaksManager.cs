using System.Linq;
using VRUIControls;
using UnityEngine;
using Zenject;
using UnityEngine.EventSystems;
using BeatLeader.Utils;

namespace BeatLeader.Replayer
{
    public class SceneTweaksManager : MonoBehaviour
    {
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly LocksController _softLocksController;
        [Inject] private readonly StandardLevelGameplayManager _gameplayManager;
        [Inject] private readonly PauseController _pauseController;
        [Inject] private readonly MainCamera _mainCamera;
        [Inject] private readonly VRInputModule _inputModule;
        [Inject] private readonly DiContainer _container;

        [Inject] private readonly IMenuButtonTrigger _pauseButtonTrigger;
        [Inject] private readonly IVRPlatformHelper _vrPlatformHelper;
        [Inject] private readonly ILevelStartController _levelStartController;

        public EventSystem CustomEventSystem { get; private set; }
        public EventSystem BaseEventSystem { get; private set; }

        private void Start()
        {
            DisableUselessStuff();
            UnsubscribeEvents();
            PatchInputSystem();

            RaycastBlocker.EnableBlocker = true;
        }
        private void OnDestroy()
        {
            RaycastBlocker.EnableBlocker = false;
        }

        private void DisableUselessStuff()
        {
            _softLocksController.InstallLock(_gameplayManager);
            _softLocksController.InstallLock(_pauseMenuManager);
            _softLocksController.InstallLock(Resources.FindObjectsOfTypeAll<CuttingManager>().First());
            _mainCamera.gameObject.SetActive(false);
            Resources.FindObjectsOfTypeAll<VRLaserPointer>().FirstOrDefault()?.gameObject.SetActive(!InputManager.IsInFPFC);
            Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().FirstOrDefault()?.gameObject.SetActive(!InputManager.IsInFPFC);
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
        private void PatchInputSystem()
        {
            BaseEventSystem = _inputModule.GetComponent<EventSystem>();
            GameObject inputSystemContainer;
            if (InputManager.IsInFPFC)
            {
                inputSystemContainer = new GameObject("2DEventSystem");
                inputSystemContainer.AddComponent<StandaloneInputModule>();
                InputManager.EnableCursor(true);
            }
            else
            {
                inputSystemContainer = Instantiate(_inputModule.gameObject);
                _container.Inject(inputSystemContainer.GetComponent<VRInputModule>());
            }
            CustomEventSystem = inputSystemContainer.GetOrAddComponent<EventSystem>();
            EventSystem.current = CustomEventSystem;
        }
    }
}
