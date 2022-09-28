using HarmonyLib;
using IPA.Loader;
using System;
using System.Reflection;

namespace BeatLeader.Interop
{
    internal static class BeatSaviorInterop
    {
        private const string submissionParameterName = "DisableBeatSaviorUpload";

        private static Assembly _pluginAssembly;
        private static Harmony _harmony;
        private static MethodInfo _setBoolMethod;
        private static MethodInfo _getBoolMethod;
        private static MethodInfo _uploadScoreMethodPostfix;
        private static MethodInfo _uploadScoreMethod;
        private static object _configInstance;
        private static bool _isMarkedToEnable;

        public static bool EnableScoreSubmission
        {
            get => (bool)_getBoolMethod?.Invoke(_configInstance,
                new object[] { "BeatSaviorData", submissionParameterName, false, true });
            set => _setBoolMethod?.Invoke(_configInstance,
                new object[] { "BeatSaviorData", submissionParameterName, !value });
        }

        public static void MarkScoreSubmissionToEnable()
        {
            _isMarkedToEnable = true;
        }

        public static void Init()
        {
            _pluginAssembly = PluginManager.GetPluginFromId("BeatSaviorData")?.Assembly;
            if (_pluginAssembly == null) return;

            try
            {
                ResolveData();

                _harmony = new Harmony("BeatLeader.Interop.BeatSavior");
                HarmonyUtils.Patch(_harmony, new HarmonyPatchDescriptor(_uploadScoreMethod, null, _uploadScoreMethodPostfix));
            }
            catch
            {
                Plugin.Log.Error("Failed to resolve BeatSavior data, replays system may submit scores to it!");
            }
        }

        private static void ResolveData()
        {
            _configInstance = _pluginAssembly.GetType("BeatSaviorData.SettingsMenu")
                .GetField("config", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            _uploadScoreMethod = _pluginAssembly.GetType("BeatSaviorData.Plugin").GetMethod(
                "UploadData", BindingFlags.Instance | BindingFlags.NonPublic);

            _setBoolMethod = _configInstance.GetType().GetMethod("SetBool",
                new Type[] { typeof(string), typeof(string), typeof(bool) });
            _getBoolMethod = _configInstance.GetType().GetMethod("GetBool",
                new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool) });

            _uploadScoreMethodPostfix = typeof(BeatSaviorInterop).GetMethod(
                nameof(UploadScorePostfix), BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static void UploadScorePostfix()
        {
            if (_isMarkedToEnable)
            {
                EnableScoreSubmission = true;
                _isMarkedToEnable = false;
            }
        }
    }
}
