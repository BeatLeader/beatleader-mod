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
        public enum Hand
        {
            Left,
            Right,
            None
        }

        [Inject] protected readonly InputManager _inputManager;
        [Inject] protected readonly MenuSabersManager _menuSabersManager;
        [Inject] protected readonly DiContainer _container;

        protected readonly Vector3 _defaultFPFCPos = new Vector3(-1.5f, -1.06f, 1f);
        protected readonly Vector3 _defaultLeftHandPos = new Vector3(2.05f, 0, 0);
        protected readonly Vector3 _defaultRightHandPos = new Vector3(-2.05f, 0, 0);
        protected readonly Vector2 _defaultFPFCFloatingSize = new Vector2(200, 200);
        protected readonly Vector2 _defaultVRFloatingSize = new Vector2(70, 30);

        protected FloatingScreen _floatingScreen;
        protected Camera _fpfcUICamera;
        protected Hand _pinnedHand;

        protected bool _fpfc;

        public FloatingScreen floatingScreen => _floatingScreen;

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

                _pinnedHand = Hand.None;
                _floatingScreen = FloatingScreen.CreateFloatingScreen(_defaultFPFCFloatingSize, false, new Vector3(), Quaternion.identity);
                _floatingScreen.transform.position = _defaultFPFCPos;
                _inputManager.SetBordersCamera(_fpfcUICamera);
            }
            else if (_inputManager.currentInputSystem == InputManager.InputSystemType.VR)
            {
                _floatingScreen = FloatingScreen.CreateFloatingScreen(_defaultVRFloatingSize, false, new Vector3(), Quaternion.identity);
                PinToHand(Hand.Left);
                _floatingScreen.transform.position = _defaultLeftHandPos;
            }
            _floatingScreen.gameObject.layer = 26;
        }
        public void PinToHand(Hand hand)
        {
            if (!_fpfc && _floatingScreen != null)
            {
                _pinnedHand = hand;
                if (_pinnedHand == Hand.Left)
                {
                    _floatingScreen.transform.SetParent(_menuSabersManager.leftController.transform);
                }
                else if (_pinnedHand == Hand.Right)
                {
                    _floatingScreen.transform.SetParent(_menuSabersManager.rightController.transform);
                }
                else if (_pinnedHand == Hand.None)
                {
                    _floatingScreen.transform.SetParent(null);
                }
                _floatingScreen.transform.localEulerAngles = new Vector3(90, 0, 0);
            }
        }
    }
}
