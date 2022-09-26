using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Interop
{
    internal static class BeatSaviorInterop
    {
        private const string submissionParameterName = "DisableBeatSaviorUpload";

        private static Assembly _pluginAssembly;
        private static MethodInfo _setBoolMethod;
        private static MethodInfo _getBoolMethod;
        private static object _configInstance;

        public static bool EnableScoreSubmission
        {
            get => (bool)_getBoolMethod?.Invoke(_configInstance, 
                new object[] { "BeatSaviorData", submissionParameterName, false, true });
            set => _setBoolMethod?.Invoke(_configInstance, 
                new object[] { "BeatSaviorData", submissionParameterName, value });
        }

        public static void Init()
        {
            _pluginAssembly = PluginManager.GetPluginFromId("BeatSaviour")?.Assembly;
            if (_pluginAssembly == null) return;

            try
            {
                ResolveData();
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
            _setBoolMethod = _configInstance.GetType().GetMethod("SetBool",
                BindingFlags.Instance | BindingFlags.Public);
            _setBoolMethod = _configInstance.GetType().GetMethod("GetBool",
                BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
