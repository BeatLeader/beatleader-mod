using BeatLeader.Attributes;
using BeatLeader.Utils;
using HarmonyLib;
using System;
using System.Reflection;

namespace BeatLeader.Interop {
    [PluginInterop("BeatSaviorData")]
    internal static class BeatSaviorInterop {
        [PluginAssembly] 
        private static readonly Assembly _assembly;

        [PluginType("BeatSaviorData.SettingsMenu")]
        private static readonly Type _settingsMenuType;

        private static Harmony _harmony;
        private static MethodInfo _setBoolMethod;
        private static MethodInfo _getBoolMethod;
        private static MethodInfo _uploadScoreMethodPostfix;
        private static MethodInfo _uploadScoreMethod;
        private static object _configInstance;
        private static bool _isMarkedToEnable;

        public static bool ScoreSubmissionEnabled {
            get => (bool)_getBoolMethod?.Invoke(_configInstance,
                new object[] { "BeatSaviorData", "DisableBeatSaviorUpload", false, true });
            set => _setBoolMethod?.Invoke(_configInstance,
                new object[] { "BeatSaviorData", "DisableBeatSaviorUpload", !value });
        }

        [InteropEntry]
        private static void Init() {
            _configInstance = _settingsMenuType.GetField("config", ReflectionUtils.StaticFlags).GetValue(null);

            _uploadScoreMethod = _assembly.GetType("BeatSaviorData.Plugin").GetMethod(
                "UploadData", ReflectionUtils.DefaultFlags);

            _setBoolMethod = _configInstance.GetType().GetMethod("SetBool",
                new Type[] { typeof(string), typeof(string), typeof(bool) });
            _getBoolMethod = _configInstance.GetType().GetMethod("GetBool",
                new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool) });

            _uploadScoreMethodPostfix = typeof(BeatSaviorInterop).GetMethod(
                nameof(UploadScorePostfix), BindingFlags.Static | BindingFlags.NonPublic);

            _harmony = new Harmony("BeatLeader.Interop.BeatSavior");
            HarmonyUtils.Patch(_harmony, new HarmonyPatchDescriptor(_uploadScoreMethod, postfix: _uploadScoreMethodPostfix));
        }
        public static void MarkScoreSubmissionToEnable() {
            _isMarkedToEnable = true;
        }
        private static void UploadScorePostfix() {
            if (_isMarkedToEnable) {
                ScoreSubmissionEnabled = true;
                _isMarkedToEnable = false;
            }
        }
    }
}
