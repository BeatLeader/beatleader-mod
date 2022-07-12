using System;
using System.Linq;
using System.Reflection;
using IPA.Utilities;
using System.Collections.Generic;
using BeatLeader.Replays.Movement;
using UnityEngine.EventSystems;
using HMUI;
using VRUIControls;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class InputManager : MonoBehaviour
    {
        public enum InputType
        {
            VR,
            FPFC
        }

        [Inject] protected readonly SoftLocksController _softLocksController;
        [Inject] protected readonly IVRPlatformHelper _platformHelper;
        [Inject] protected readonly VRInputModule _inputModule;
        [Inject] protected readonly DiContainer _container;

        public event Action<EventSystem> onEventSystemChanged;

        public InputType currentInputType { get; protected set; }
        public EventSystem currentEventSystem { get; protected set; }
        public EventSystem customEventSystem { get; protected set; }
        public EventSystem baseEventSystem { get; protected set; }
        public bool isInFPFC => currentInputType == InputType.FPFC;

        private void Awake()
        {
            baseEventSystem = _inputModule.GetComponent<EventSystem>();
            _softLocksController.InstallLock(baseEventSystem, SoftLocksController.LockMode.WhenRequired);
            GameObject inputSystemContainer;
            EventSystem eventSystem;
            if (_platformHelper.GetType() == typeof(DevicelessVRHelper))
            {
                currentInputType = InputType.FPFC;
                inputSystemContainer = new GameObject("EventSystem");
                eventSystem = inputSystemContainer.AddComponent<EventSystem>();
                inputSystemContainer.AddComponent<StandaloneInputModule>();
                EnableCursor(true);
            }
            else
            {
                currentInputType = InputType.VR;
                inputSystemContainer = Instantiate(_inputModule.gameObject);
                eventSystem = inputSystemContainer.GetComponent<EventSystem>();
                inputSystemContainer.gameObject.SetActive(true);
                _container.Inject(inputSystemContainer.GetComponent<VRInputModule>());
            }
            customEventSystem = eventSystem;
        }
        public void EnableInput(bool enable)
        {
            _softLocksController.Lock(baseEventSystem, currentEventSystem == baseEventSystem ? !enable : true);
            currentEventSystem.enabled = enable;
        }
        public void SwitchInputTo(bool original = true)
        {
            _softLocksController.Lock(baseEventSystem, !original, true);
            customEventSystem.enabled = !original;
            onEventSystemChanged?.Invoke(currentEventSystem = original ? baseEventSystem : customEventSystem);
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
