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

        public InputSystemType currentInputSystem => _currentInputSystem;
        public bool isInFPFC => _currentInputSystem == InputSystemType.FPFC;

        public void Awake()
        {
            DisableBaseInput();
            if (_platformHelper.GetType() == typeof(DevicelessVRHelper))
            {
                _currentInputSystem = InputSystemType.FPFC;
                EnableCursor();
            }
            else
                _currentInputSystem = InputSystemType.VR;
            CreateInputSystem(_currentInputSystem);
        }
        protected virtual void CreateInputSystem(InputSystemType type)
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
        }
        protected virtual void DisableBaseInput()
        {
            _softLocksController.InstallLock(_inputModule.GetComponent<EventSystem>(), SoftLocksController.LockMode.WhenRequired);
        }

        public static void SwitchCursor()
        {
            if (Cursor.visible)
                DisableCursor();
            else
                EnableCursor();
        }
        public static void EnableCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        public static void DisableCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
