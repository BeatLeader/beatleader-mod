using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Managers;
using VRUIControls;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Managers
{
    public class MultiplatformUIManager : MonoBehaviour
    {
        public enum PinPose
        {
            Left,
            Right,
            None
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly MenuSabersManager _menuSabersManager;

        protected readonly Vector3 _defaultFPFCPos = new Vector3(-1.5f, -1.06f, 1f);
        protected readonly Vector3 _defaultLeftPinPosePos = new Vector3(0.75f, 0, 0);
        protected readonly Vector3 _defaultRightPinPosePos = new Vector3(-0.75f, 0, 0);
        protected readonly Vector2 _defaultFPFCFloatingSize = new Vector2(200, 200);
        protected readonly Vector2 _defaultVRFloatingSize = new Vector2(30, 50);

        protected FloatingScreen _floatingScreen;
        protected Camera _fpfcUICamera;
        protected PinPose _pinPose;

        protected bool _fpfc;

        public FloatingScreen floatingScreen => _floatingScreen;
        public PinPose pinPose => _pinPose;

        public void Start()
        {
            if (_inputManager.currentInputSystem == InputManager.InputSystemType.FPFC)
            {
                _fpfc = true;
                _fpfcUICamera = new GameObject("ReplayerGUICamera").AddComponent<Camera>();
                _fpfcUICamera.depth = 15;
                _fpfcUICamera.cullingMask = 67108864;
                _fpfcUICamera.orthographicSize = 2;
                _fpfcUICamera.orthographic = true;
                _fpfcUICamera.clearFlags = CameraClearFlags.Depth;

                _pinPose = PinPose.None;
                _floatingScreen = FloatingScreen.CreateFloatingScreen(_defaultFPFCFloatingSize, false, new Vector3(), Quaternion.identity);
                _floatingScreen.transform.localPosition = _defaultFPFCPos;
                _floatingScreen.gameObject.layer = 26;
                _inputManager.SetBordersCamera(_fpfcUICamera);
            }
            else if (_inputManager.currentInputSystem == InputManager.InputSystemType.VR)
            {
                _floatingScreen = FloatingScreen.CreateFloatingScreen(_defaultVRFloatingSize, false, new Vector3(), Quaternion.identity);
                PinToPose(PinPose.Left);
                _floatingScreen.transform.localPosition = _defaultLeftPinPosePos;
            }
        }
        public void PinToPose(PinPose PinPose)
        {
            if (!_fpfc && _floatingScreen != null)
            {
                _pinPose = PinPose;
                if (_pinPose == PinPose.Left)
                {
                    _floatingScreen.transform.SetParent(_menuSabersManager.leftController.transform);
                    _floatingScreen.transform.localPosition = _defaultLeftPinPosePos;
                }
                else if (_pinPose == PinPose.Right)
                {
                    _floatingScreen.transform.SetParent(_menuSabersManager.rightController.transform);
                    _floatingScreen.transform.localPosition = _defaultRightPinPosePos;
                }
                else if (_pinPose == PinPose.None)
                {
                    _floatingScreen.transform.SetParent(null);
                }
                _floatingScreen.transform.localEulerAngles = new Vector3(90, 0, 0);
            }
        }
    }
}
