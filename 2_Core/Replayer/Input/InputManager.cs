using System;
using IPA.Utilities;
using Libraries.HM.HMLib.VR;
using UnityEngine.EventSystems;
using VRUIControls;
using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader.Replayer
{
    //TODO: rewrite
    public class InputManager : MonoBehaviour
    {
        [Flags]
        public enum InputType
        {
            VR = 1,
            FPFC = 2
        }

        [Inject] private readonly VRInputModule _inputModule;
        [Inject] private readonly DiContainer _container;

        public InputType CurrentInputType { get; private set; }
        public EventSystem CurrentEventSystem { get; private set; }
        public EventSystem CustomEventSystem { get; private set; }
        public EventSystem BaseEventSystem { get; private set; }
        public static bool IsInFPFC => Environment.GetCommandLineArgs().Contains("fpfc");

        public event Action<EventSystem> OnEventSystemChanged;

        private void Awake()
        {
            BaseEventSystem = _inputModule.GetComponent<EventSystem>();
            GameObject inputSystemContainer;
            EventSystem eventSystem;
            if (IsInFPFC)
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

                var comparableInputModule = inputSystemContainer.AddComponent<ComparableVRInputModule>();
                comparableInputModule.SetField<VRInputModule, VRPointer>("_vrPointer", pointer);
                comparableInputModule.SetField<VRInputModule, HapticPresetSO>("_rumblePreset", rumblePreset);
                _container.Inject(comparableInputModule);

                eventSystem.SetField("m_CurrentInputModule", (BaseInputModule)comparableInputModule);
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

        public static bool MatchesCurrentInput(InputType type)
        {
            return type.HasFlag(IsInFPFC ? InputType.FPFC : InputType.VR);
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