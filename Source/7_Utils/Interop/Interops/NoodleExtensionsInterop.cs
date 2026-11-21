using BeatLeader.Attributes;
using BeatLeader.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace BeatLeader.Interop {
    [PluginInterop("NoodleExtensions")]
    internal static class NoodleExtensionsInterop {
        [PluginAssembly] 
        private static readonly Assembly _assembly;

        [PluginType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager")]
        private static readonly Type _neCallbacksControllerType;

        [PluginType("NoodleExtensions.NoodleObjectData")]
        private static readonly Type _neObjectDataType;

        private static HarmonyAutoPatch _callbacksControllerPatch;
        private static MethodInfo _neCallbacksControllerUpdateMethod;
        private static MethodInfo _prefixMethod;
        private static FieldInfo _callbacksInTimeField;
        private static FieldInfo _prevSongTimeField;
        private static FieldInfo _deserializedDataField;
        private static PropertyInfo _objectCustomDatasProperty;
        private static PropertyInfo _njsProperty;
        private static PropertyInfo _spawnOffsetProperty;
        private static PropertyInfo _aheadTimeProperty;
        private static IDictionary? _objectCustomDatas;
        private static bool _neReprocessRequested;

        [InteropEntry]
        private static void Init() {
            _prevSongTimeField = _neCallbacksControllerType
                 .GetField("_prevSongtime", ReflectionUtils.DefaultFlags);
            _callbacksInTimeField = _neCallbacksControllerType
                .GetField("_callbacksInTime", ReflectionUtils.DefaultFlags);
            _deserializedDataField = _neCallbacksControllerType
                .GetField("_deserializedData", ReflectionUtils.DefaultFlags);
            _objectCustomDatasProperty = _deserializedDataField.FieldType
                .GetProperty("ObjectCustomDatas", ReflectionUtils.DefaultFlags);
            _njsProperty = _neObjectDataType.GetProperty("Njs", ReflectionUtils.DefaultFlags);
            _spawnOffsetProperty = _neObjectDataType.GetProperty("SpawnOffset", ReflectionUtils.DefaultFlags);
            _aheadTimeProperty = _neObjectDataType.GetProperty("InternalAheadTime", ReflectionUtils.DefaultFlags);
            _neCallbacksControllerUpdateMethod = _neCallbacksControllerType
                .GetMethod("ManualUpdate", ReflectionUtils.DefaultFlags);
            _prefixMethod = typeof(NoodleExtensionsInterop).GetMethod(
                nameof(CallbacksControllerPrefix), ReflectionUtils.StaticFlags);

            _callbacksControllerPatch = new HarmonyPatchDescriptor(_neCallbacksControllerUpdateMethod, _prefixMethod);
        }
        public static void RequestReprocess() {
            _neReprocessRequested = true;
            _objectCustomDatas = null;
        }
        public static void ClearObjectData() {
            _objectCustomDatas = null;
        }
        public static bool TryGetSpawnAheadTime(BeatmapObjectData beatmapObjectData, out float aheadTime) {
            aheadTime = 0f;
            var objectData = _objectCustomDatas?[beatmapObjectData];
            if (objectData == null || !_neObjectDataType.IsInstanceOfType(objectData)) return false;
            if (_njsProperty.GetValue(objectData) == null && _spawnOffsetProperty.GetValue(objectData) == null) return false;
            if (_aheadTimeProperty.GetValue(objectData) is not float time) return false;
            aheadTime = time;
            return true;
        }
        private static void CallbacksControllerPrefix(object __instance) {
            if (_neReprocessRequested) {
                try {
                    _prevSongTimeField.SetValue(__instance, float.MinValue);
                    ((CallbacksInTime)_callbacksInTimeField.GetValue(__instance)).lastProcessedNode = null;
                    _objectCustomDatas = (IDictionary)_objectCustomDatasProperty.GetValue(_deserializedDataField.GetValue(__instance));
                    _neReprocessRequested = false;
                } catch (Exception ex) {
                    Plugin.Log.Error("Failed to update NE callbacks! \r\n" + ex);
                }
            }
        }
    }
}
