using BeatLeader.Attributes;
using BeatLeader.Utils;
using HarmonyLib;
using System;
using System.Reflection;

namespace BeatLeader.Interop {
    [PluginInterop("NoodleExtensions")]
    internal static class NoodleExtensionsInterop {
        [PluginAssembly] 
        private static readonly Assembly _assembly;

        [PluginType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager")]
        private static readonly Type _neCallbacksControllerType;

        private static Harmony _harmony;
        private static MethodInfo _neCallbacksControllerUpdateMethod;
        private static MethodInfo _prefixMethod;
        private static FieldInfo _callbacksInTimeField;
        private static FieldInfo _prevSongTimeField;
        private static bool _neReprocessRequested;

        [InteropEntry]
        private static void Init() {
            _prevSongTimeField = _neCallbacksControllerType
                 .GetField("_prevSongtime", ReflectionUtils.DefaultFlags);
            _callbacksInTimeField = _neCallbacksControllerType
                .GetField("_callbacksInTime", ReflectionUtils.DefaultFlags);
            _neCallbacksControllerUpdateMethod = _neCallbacksControllerType
                .GetMethod("ManualUpdate", ReflectionUtils.DefaultFlags);
            _prefixMethod = typeof(NoodleExtensionsInterop).GetMethod(
                nameof(NoodleCallbacksControllerPrefix), ReflectionUtils.StaticFlags);

            _harmony = new("BeatLeader.Interop.NoodleExtensions");
            HarmonyUtils.Patch(_harmony, new HarmonyPatchDescriptor(_neCallbacksControllerUpdateMethod, _prefixMethod));
        }
        public static void RequestReprocess() {
            _neReprocessRequested = true;
        }

        private static void NoodleCallbacksControllerPrefix(object __instance) {
            if (_neReprocessRequested) {
                _prevSongTimeField.SetValue(__instance, float.MinValue);
                ((CallbacksInTime)_callbacksInTimeField.GetValue(__instance)).lastProcessedNode = null;
                _neReprocessRequested = false;
            }
        }
    }
}
