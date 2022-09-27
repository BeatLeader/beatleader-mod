using System.Linq;
using VRUIControls;
using UnityEngine;
using Zenject;
using UnityEngine.EventSystems;
using BeatLeader.Utils;
using IPA.Utilities;
using BeatLeader.Replayer.Camera;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeatLeader.Replayer
{
    public class SceneTweaksManager : MonoBehaviour
    {
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly ReplayerCameraController _cameraController;
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
            try
            {
                DisableUselessStuff();
                UnsubscribeEvents();
                PatchInputSystem();
                if (!InputManager.IsInFPFC) PatchSmoothCamera();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Failed to patch some gameplay stuff, replays system may work not properly!" + "\r\n" + ex);
            }

            RaycastBlocker.EnableBlocker = true;
        }
        private void OnDestroy()
        {
            _multisilencer.Dispose();

            RaycastBlocker.EnableBlocker = false;
            RaycastBlocker.ReleaseMemory();
        }

        private void DisableUselessStuff()
        {
            Resources.FindObjectsOfTypeAll<VRLaserPointer>().FirstOrDefault()?.gameObject.SetActive(!InputManager.IsInFPFC);
            Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().FirstOrDefault()?.gameObject.SetActive(!InputManager.IsInFPFC);

            _mainCamera.gameObject.SetActive(false);
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
                inputSystemContainer = GameObject.Instantiate(_inputModule.gameObject);
                _container.Inject(inputSystemContainer.GetComponent<VRInputModule>());
            }
            CustomEventSystem = inputSystemContainer.GetOrAddComponent<EventSystem>();
            EventSystem.current = CustomEventSystem;
        }
        private void PatchSmoothCamera()
        {
            var smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(x => x.transform.parent.name == "LocalPlayerGameCore");
            if (smoothCamera == null) return;

            var fakeCam = new MainCamera();
            fakeCam.SetField("_camera", _cameraController.Camera);
            fakeCam.SetField("_transform", _cameraController.Camera.transform);
            smoothCamera.SetField("_mainCamera", fakeCam);
            smoothCamera.gameObject.SetActive(true);
        }

        #region Harmony

        private static readonly IReadOnlyList<MethodInfo> silencedMethods = new[]
        {
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.ShowMenu), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.Update), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.StartResumeAnimation), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.RestartButtonPressed), ReflectionUtils.DefaultFlags),
            typeof(PauseMenuManager).GetMethod(nameof(PauseMenuManager.ContinueButtonPressed), ReflectionUtils.DefaultFlags),

            typeof(StandardLevelGameplayManager).GetMethod(nameof(StandardLevelGameplayManager.Update), ReflectionUtils.DefaultFlags),
            typeof(CuttingManager).GetMethod(nameof(CuttingManager.HandleSaberManagerDidUpdateSaberPositions), ReflectionUtils.DefaultFlags),
        };
        
        private HarmonyMultisilencer _multisilencer = new(silencedMethods);

        #endregion 
    }
}
