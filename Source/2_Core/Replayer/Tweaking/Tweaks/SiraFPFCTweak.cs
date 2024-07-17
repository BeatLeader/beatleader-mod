using System;
using System.Collections;
using System.Reflection;
using BeatLeader.Utils;
using SiraUtil.Tools.FPFC;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class SiraFPFCTweak : GameTweak {
        [Inject] private readonly IFPFCSettings _fpfcSettings = null!;
        [Inject] private readonly DiContainer _container = null!;

        private HarmonySilencer _tickSilencer = null!;
        private HarmonyAutoPatch? _togglePatch;

        private static MethodInfo? _togglePatchMethod;
        private static MethodInfo? _togglePrefixMethod;

        private static MethodInfo? _settingsControllerTickMethod;
        private static FieldInfo? _cameraControllerField;
        private static Type? _cameraListenerType;
        private static Type? _toggleListenerType;
        private static bool _isInitialized;

        public override void Initialize() {
            InitIfNeeded();
            var listeners = _container.ResolveAll<IAsyncInitializable>();
            _container.UnbindInterfacesTo(_cameraListenerType);
            _container.UnbindInterfacesTo(_toggleListenerType);
            //if in fpfc disabling fpfc camera controller
            if (_fpfcSettings.Enabled) {
                var toggle = FindToggle(listeners);
                var cameraController = _cameraControllerField!.GetValue(toggle) as MonoBehaviour;
                //preventing further enabling if not enabled yet
                if (cameraController == null) {
                    _togglePatch = new HarmonyPatchDescriptor(_togglePatchMethod!, _togglePrefixMethod);
                } else {
                    cameraController.enabled = false;
                }
            }
            _tickSilencer = new(_settingsControllerTickMethod!);
        }

        public override void Dispose() {
            _tickSilencer.Dispose();
            _togglePatch?.Dispose();
        }

        private static bool ToggleEnablePrefix() {
            return false;
        }

        private static object FindToggle(IEnumerable listeners) {
            object? toggle = null;
            foreach (var listener in listeners) {
                if (listener.GetType() != _toggleListenerType) continue;
                toggle = listener;
            }
            return toggle ?? throw new InvalidOperationException("The toggle type was not represented in the container");
        }

        private static void InitIfNeeded() {
            if (_isInitialized) return;
            var assembly = typeof(IFPFCSettings).Assembly;
            var controllerType = assembly.GetType("SiraUtil.Tools.FPFC.FPFCSettingsController");
            //
            _settingsControllerTickMethod = controllerType.GetMethod("Tick", ReflectionUtils.DefaultFlags);
            _cameraListenerType = assembly.GetType("SiraUtil.Tools.FPFC.SmoothCameraListener");
            _toggleListenerType = assembly.GetType("SiraUtil.Tools.FPFC.FPFCToggle");
            _cameraControllerField = _toggleListenerType.GetField("_simpleCameraController", ReflectionUtils.DefaultFlags);
            //
            _togglePrefixMethod = typeof(SiraFPFCTweak).GetMethod(nameof(ToggleEnablePrefix), ReflectionUtils.StaticFlags);
            _togglePatchMethod = _toggleListenerType.GetMethod("EnableFPFC", ReflectionUtils.DefaultFlags);
            //
            _isInitialized = true;
        }
    }
}