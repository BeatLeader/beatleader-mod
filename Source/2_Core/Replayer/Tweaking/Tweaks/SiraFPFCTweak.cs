using System;
using System.Reflection;
using BeatLeader.Utils;
using SiraUtil.Tools.FPFC;

namespace BeatLeader.Replayer.Tweaking {
    internal class SiraFPFCTweak : GameTweak {
        private HarmonySilencer? _toggleSilencer;
        private HarmonySilencer? _cameraSilencer;
        private HarmonySilencer? _smoothCameraSilencer;

        private static MethodInfo? _toggleActionMethod;
        private static MethodInfo? _cameraUpdateMethod;
        private static MethodInfo? _smoothCameraUpdateMethod;
        private static bool _isInitialized;

        public override void Initialize() {
            InitIfNeeded();
            //preventing toggling fpfc using the toggle key
            _toggleSilencer = new(_toggleActionMethod!);
            //stopping the fpfc camera controller from moving the camera
            _cameraSilencer = new(_cameraUpdateMethod!);
            //preventing the smooth camera from being activated by fpfc
            _smoothCameraSilencer = new(_smoothCameraUpdateMethod!);
        }

        public override void Dispose() {
            _toggleSilencer?.Dispose();
            _cameraSilencer?.Dispose();
            _smoothCameraSilencer?.Dispose();
        }

        private static MethodInfo GetMethod(Assembly assembly, string typeName, string methodName) {
            var type = assembly.GetType($"SiraUtil.Tools.FPFC.{typeName}")
                ?? throw new InvalidOperationException($"{typeName} was not found");
            return type.GetMethod(methodName, ReflectionUtils.DefaultFlags)
                ?? throw new InvalidOperationException($"{typeName}.{methodName} was not found");
        }

        private static void InitIfNeeded() {
            if (_isInitialized) return;
            var assembly = typeof(IFPFCSettings).Assembly;
            _toggleActionMethod = GetMethod(assembly, "FPFCSettingsController", "OnToggleActionPerformed");
            _cameraUpdateMethod = GetMethod(assembly, "CameraController", "OnBeforeUpdate");
            _smoothCameraUpdateMethod = GetMethod(assembly, "SmoothCameraListener", "UpdateState");
            _isInitialized = true;
        }
    }
}
