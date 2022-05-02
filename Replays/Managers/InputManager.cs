using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Movement;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class InputManager : MonoBehaviour
    {
        [Inject] protected readonly VRControllersInputManager _vrControllersInputManager;
        [Inject] protected readonly MenuSabersManager _menuSabersManager;
        [Inject] protected readonly PlaybackUIController _playbackUIController;
        [Inject] protected readonly IVRPlatformHelper _platformHelper;

        public enum InputSystemType
        {
            VR,
            FPFC
        }

        protected InputSystemType _currentInputSystem;
        protected FakeVRController _pointer;
        protected VRPointer _scenePointer;

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
            }
            else
            {
                _currentInputSystem = InputSystemType.VR;


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
                if (Cursor.visible != true || Cursor.lockState != CursorLockMode.None)
                {
                    AppearCursor();
                }
                if (_scenePointer != null)
                {
                    if (_scenePointer.vrController != _pointer) ReplacePointer(_pointer);
                }
                else if (_menuSabersManager.initialized)
                {
                    _scenePointer = Resources.FindObjectsOfTypeAll<VRPointer>().First(x => x.vrController == _menuSabersManager.rightController);
                }
                if (_playbackUIController.initialized)
                {
                    _pointer.transform.position = _playbackUIController.uiCamera.ScreenToWorldPoint(Input.mousePosition);
                }
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
        public void AppearInputSystem()
        {
            if (_currentInputSystem == InputSystemType.VR)
            {

            }
            else if (_currentInputSystem == InputSystemType.FPFC)
            {

            }
        }
        public void DisappearInputSystem()
        {
            if (_currentInputSystem == InputSystemType.VR)
            {

            }
            else if (_currentInputSystem == InputSystemType.FPFC)
            {

            }
        }
        public void ReplacePointer(VRController pointerVRController)
        {
            _scenePointer.GetType().GetField("_vrController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_scenePointer, _pointer);
        }
    }
}
