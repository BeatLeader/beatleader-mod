using System;
using System.Linq;
using System.Reflection;
using IPA.Utilities;
using System.Collections.Generic;
using BeatLeader.Replayer.Movement;
using UnityEngine.EventSystems;
using HMUI;
using VRUIControls;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Managers
{
    public class InputManager : MonoBehaviour
    {
        [Flags]
        public enum InputType
        {
            VR = 1,
            FPFC = 2
        }

        [Inject] private readonly SoftLocksController _softLocksController;
        [Inject] private readonly IVRPlatformHelper _platformHelper;
        [Inject] private readonly VRInputModule _inputModule;
        [Inject] private readonly DiContainer _container;

        public event Action<EventSystem> OnEventSystemChanged;

        public InputType CurrentInputType { get; private set; }
        public EventSystem CurrentEventSystem { get; private set; }
        public EventSystem CustomEventSystem { get; private set; }
        public EventSystem BaseEventSystem { get; private set; }
        public bool IsInFPFC => CurrentInputType == InputType.FPFC;

        private void Awake()
        {
            BaseEventSystem = _inputModule.GetComponent<EventSystem>();
            _softLocksController.InstallLock(BaseEventSystem, SoftLocksController.LockMode.WhenRequired);
            GameObject inputSystemContainer;
            EventSystem eventSystem;
            if (_platformHelper.GetType() == typeof(DevicelessVRHelper))
            {
                CurrentInputType = InputType.FPFC;
                inputSystemContainer = new GameObject("EventSystem");
                eventSystem = inputSystemContainer.AddComponent<EventSystem>();
                inputSystemContainer.AddComponent<StandaloneInputModule>();
                EnableCursor(true);
            }
            else
            {
                CurrentInputType = InputType.VR;
                inputSystemContainer = Instantiate(_inputModule.gameObject);
                eventSystem = inputSystemContainer.GetComponent<EventSystem>();
                inputSystemContainer.gameObject.SetActive(true);
                _container.Inject(inputSystemContainer.GetComponent<VRInputModule>());
            }
            CustomEventSystem = eventSystem;
        }
        public void EnableInput(bool enable)
        {
            _softLocksController.Lock(BaseEventSystem, CurrentEventSystem == BaseEventSystem ? !enable : true);
            CurrentEventSystem.enabled = enable;
        }
        public void SwitchInputTo(bool original = true)
        {
            _softLocksController.Lock(BaseEventSystem, !original, true);
            CustomEventSystem.enabled = !original;
            OnEventSystemChanged?.Invoke(CurrentEventSystem = original ? BaseEventSystem : CustomEventSystem);
        }
        public bool MatchesCurrentInput(InputType type)
        {
            return type.HasFlag(CurrentInputType);
        }

        public static void SwitchCursor()
        {
            EnableCursor(!Cursor.visible);
        }
        public static void EnableCursor(bool enable)
        {
            Cursor.visible = enable;
            Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
