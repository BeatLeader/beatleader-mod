using HarmonyLib;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Interop
{
    internal static class NoodleExtensionsInterop
    {
        private static MethodInfo _neCallbacksControllerUpdateMethod;
        private static MethodInfo _prefixMethod;
        private static Type _neCallbacksControllerType;
        private static Assembly _neAssembly;
        private static Harmony _harmony;
        private static FieldInfo _callbacksInTimeField;
        private static FieldInfo _prevSongTimeField;
        private static bool _neReprocessRequested;

        public static void RequestReprocess()
        {
            _neReprocessRequested = true;
        }
        public static void Init()
        {
            _neAssembly = PluginManager.GetPluginFromId("NoodleExtensions")?.Assembly;
            if (_neAssembly == null) return;

            try
            {
                ResolveData();
                _harmony = new Harmony("BeatLeader.Replayer.NEInterop");
                HarmonyUtils.Patch(_harmony, new HarmonyPatchDescriptor(_neCallbacksControllerUpdateMethod, _prefixMethod));
            }
            catch
            {
                Plugin.Log.Error("Failed to resolve NoodleExtensions data, replays system may work not properly!");
            }
        }
        private static void ResolveData()
        {
            _neCallbacksControllerType = _neAssembly.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            _prevSongTimeField = _neCallbacksControllerType.GetField("_prevSongtime", flags);
            _callbacksInTimeField = _neCallbacksControllerType.GetField("_callbacksInTime", flags);

            _neCallbacksControllerUpdateMethod = _neCallbacksControllerType.GetMethod("ManualUpdate", flags);
            _prefixMethod = typeof(NoodleExtensionsInterop).GetMethod(nameof(NoodleCallbacksControllerPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static void NoodleCallbacksControllerPrefix(object __instance)
        {
            if (_neReprocessRequested)
            {
                _prevSongTimeField.SetValue(__instance, float.MinValue);
                ((CallbacksInTime)_callbacksInTimeField.GetValue(__instance)).lastProcessedNode = null;
                _neReprocessRequested = false;
            }
        }
    }
}
