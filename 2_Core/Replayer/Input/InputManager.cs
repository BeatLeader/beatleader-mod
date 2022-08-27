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
using Libraries.HM.HMLib.VR;

namespace BeatLeader.Replayer
{
    public class InputManager : MonoBehaviour
    {
        [Flags]
        public enum InputType
        {
            VR = 1,
            FPFC = 2
        }

        [Inject] private readonly IVRPlatformHelper _platformHelper;
        [Inject] private readonly VRInputModule _inputModule;
        [Inject] private readonly DiContainer _container;

        public InputType CurrentInputType { get; private set; }
        public EventSystem CurrentEventSystem { get; private set; }
        public EventSystem CustomEventSystem { get; private set; }
        public EventSystem BaseEventSystem { get; private set; }
        public ComparableVRInputModule VRInputModule { get; private set; }
        public bool IsInFPFC => CurrentInputType == InputType.FPFC;

        public event Action<EventSystem> OnEventSystemChanged;

        private void Awake()
        {
            BaseEventSystem = _inputModule.GetComponent<EventSystem>();
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
                inputSystemContainer.gameObject.SetActive(false);

                var originalInputModule = inputSystemContainer.GetComponent<VRInputModule>();
                var pointer = originalInputModule.GetField<VRPointer, VRInputModule>("_vrPointer");
                var rumblePreset = originalInputModule.GetField<HapticPresetSO, VRInputModule>("_rumblePreset");
                GameObject.DestroyImmediate(originalInputModule);
                
                VRInputModule = inputSystemContainer.AddComponent<ComparableVRInputModule>();
                VRInputModule.SetField<VRInputModule, VRPointer>("_vrPointer", pointer);
                VRInputModule.SetField<VRInputModule, HapticPresetSO>("_rumblePreset", rumblePreset);
                _container.Inject(VRInputModule);
                
                eventSystem.SetField("m_CurrentInputModule", (BaseInputModule)VRInputModule);
                inputSystemContainer.gameObject.SetActive(true);
            }
            CustomEventSystem = eventSystem;
            SwitchInputTo(false);
        }
        public void EnableInput(bool enable)
        {
            CurrentEventSystem.enabled = enable;
        }
        public void SwitchInputTo(bool original = true)
        {
            var current = CurrentEventSystem;
            EventSystem.current = original ? BaseEventSystem : CustomEventSystem;
            OnEventSystemChanged?.Invoke(current);
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
