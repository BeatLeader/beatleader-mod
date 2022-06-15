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
        public enum InputSystemType
        {
            VR,
            FPFC
        }

        [Inject] protected readonly SoftLocksController _softLocksController;
        [Inject] protected readonly IVRPlatformHelper _platformHelper;
        [Inject] protected readonly VRInputModule _inputModule;
        [Inject] protected readonly DiContainer _container;

        protected InputSystemType _currentInputSystem;
        protected GameObject _inputSystemContainer;
        protected EventSystem _baseEventSystem;
        protected EventSystem _customEventSystem;

        public InputSystemType currentInputSystem => _currentInputSystem;
        public bool isInFPFC => _currentInputSystem == InputSystemType.FPFC;

        public void Awake()
        {
            _baseEventSystem = _inputModule.GetComponent<EventSystem>();
            _softLocksController.InstallLock(_baseEventSystem, SoftLocksController.LockMode.WhenRequired);
            if (_platformHelper.GetType() == typeof(DevicelessVRHelper))
            {
                _currentInputSystem = InputSystemType.FPFC;
                EnableCursor(true);
            }
            else
                _currentInputSystem = InputSystemType.VR;
            CreateInputSystem(_currentInputSystem);
        }
        public void EnableCustomInput(bool enable)
        {
            _customEventSystem.enabled = enable;
        }
        public void EnableBaseInput(bool enable)
        {
            _softLocksController.Lock(enable, _baseEventSystem);
        }
        protected void CreateInputSystem(InputSystemType type)
        {
            if (type == InputSystemType.FPFC)
            {
                _inputSystemContainer = new GameObject("EventSystem");
                _inputSystemContainer.AddComponent<EventSystem>();
                _inputSystemContainer.AddComponent<StandaloneInputModule>();
            }
            else if (type == InputSystemType.VR)
            {
                _inputSystemContainer = Instantiate(_inputModule.gameObject);
                _inputSystemContainer.gameObject.SetActive(true);
                _container.Inject(_inputSystemContainer.GetComponent<VRInputModule>());
            }
            _customEventSystem = _inputSystemContainer.GetComponent<EventSystem>();
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
