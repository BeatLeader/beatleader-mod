using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiraUtil.Tools.FPFC;
using BeatLeader.Replays.Movement;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class InputManager : MonoBehaviour
    {
        [Inject] protected readonly VRControllersInputManager _vrControllersInputManager;
        [Inject] protected readonly VRInputModule _inputModule;
        [Inject] protected readonly MainCamera _mainCamera;
        [Inject] protected readonly MenuSabersManager _menuSabersManager;
        [Inject] protected readonly IFPFCSettings _fpfcSettings;
        [Inject] protected readonly IVRPlatformHelper _platformHelper;

        public enum InputSystemType
        {
            VR,
            FPFC
        }

        protected FakeVRController _pointer;
        protected VRPointer _scenePointer;
        protected Camera _inputBordersCamera;

        protected InputSystemType _currentInputSystem;
        protected bool _cursorWantsToAppear;

        public InputSystemType currentInputSystem => _currentInputSystem;

        public void Awake()
        {
            if (_platformHelper.GetType() == typeof(DevicelessVRHelper))
            {
                _currentInputSystem = InputSystemType.FPFC;

                _pointer = new GameObject("Replayer2DPointer").AddComponent<FakeVRController>();
                _pointer.node = UnityEngine.XR.XRNode.GameController;

                _pointer.GetType().GetField("_vrControllersInputManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_pointer, _vrControllersInputManager);
                AppearCursor();
                //_fpfcSettings.Enabled = false;
                _cursorWantsToAppear = true;
            }
            else
            {
                _currentInputSystem = InputSystemType.VR;
                EnableInputSystem();
            }
        }
        public void OnDestroy()
        {
            DisappearCursor();
        }
        public void LateUpdate()
        {
            if (_pointer != null && _currentInputSystem == InputSystemType.FPFC)
            {
                if (_cursorWantsToAppear) AppearCursor();         
                if (_scenePointer != null)
                {
                    if (_scenePointer.vrController != _pointer) ReplacePointer(_pointer);
                }
                else if (_menuSabersManager.initialized)
                {
                    _scenePointer = Resources.FindObjectsOfTypeAll<VRPointer>().First(x => x.vrController == _menuSabersManager.rightController);
                }
                if (_inputBordersCamera != null)
                {
                    _pointer.transform.position = _inputBordersCamera.ScreenToWorldPoint(Input.mousePosition);
                }
                //Debug.LogWarning(_pointer.enabled);
                //if (Input.GetKeyDown(KeyCode.V))
                //{
                //    Debug.LogWarning("V pressed");
                //    if (_cursorWantsToAppear)
                //    {
                //        _cursorWantsToAppear = false;
                //        DisableInputSystem();
                //        DisappearCursor();
                //    }
                //    else
                //    {
                //        _cursorWantsToAppear = true;
                //        EnableInputSystem();
                //        AppearCursor();
                //    }
                //}
                //Debug.LogWarning(_mainCamera.transform.rotation);
                //Debug.LogWarning(_mainCamera.transform.localEulerAngles);
            }
            else if (_currentInputSystem == InputSystemType.VR)
            {
                EnableInputSystem();
            }
        }
        public void AppearCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        public void DisappearCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void EnableInputSystem()
        {
            if (_currentInputSystem == InputSystemType.VR)
            {
                _inputModule.enabled = true;
            }
            else if (_currentInputSystem == InputSystemType.FPFC)
            {
                _pointer.gameObject.SetActive(true);
            }
        }
        public void DisableInputSystem()
        {
            if (_currentInputSystem == InputSystemType.VR)
            {
                _inputModule.enabled = false;
            }
            else if (_currentInputSystem == InputSystemType.FPFC)
            {
                _pointer.gameObject.SetActive(false);
            }
        }
        public void ReplacePointer(VRController pointerVRController)
        {
            _scenePointer.GetType().GetField("_vrController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_scenePointer, _pointer);
        }
        public void SetBordersCamera(Camera camera)
        {
            _inputBordersCamera = camera;
        }
    }
}
