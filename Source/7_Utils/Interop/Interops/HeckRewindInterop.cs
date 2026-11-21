using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BeatLeader.Attributes;
using BeatLeader.Utils;
using HarmonyLib;
using Zenject;

namespace BeatLeader.Interop {
    [PluginInterop("Heck")]
    internal static class HeckRewindInterop {
        [PluginState]
        public static bool IsInstalled { get; private set; }

        [PluginType("Heck.ReLoad.ReLoader")]
        private static Type _reLoaderType = null!;

        [PluginType("Heck.Animation.Track")]
        private static Type _trackType = null!;

        [PluginType("Heck.HeckController")]
        private static Type _heckControllerType = null!;

        public static Type CustomEventDataType { get; private set; } = null!;

        private static Type _customBeatmapDataType = null!;
        private static Type _tracksType = null!;
        private static FieldInfo _rewindedField = null!;
        private static MethodInfo _nullPropertiesMethod = null!;
        private static PropertyInfo _debugModeProperty = null!;

        [InteropEntry]
        private static void Init() {
            _rewindedField = _reLoaderType.GetField("Rewinded", ReflectionUtils.DefaultFlags)
                ?? throw new MissingFieldException(_reLoaderType.Name, "Rewinded");
            _nullPropertiesMethod = _trackType.GetMethod("NullProperties", ReflectionUtils.DefaultFlags)
                ?? throw new MissingMethodException(_trackType.Name, "NullProperties");
            _debugModeProperty = _heckControllerType.GetProperty("DebugMode", ReflectionUtils.StaticFlags)
                ?? throw new MissingMemberException(_heckControllerType.Name, "DebugMode");
            _customBeatmapDataType = AccessTools.TypeByName("CustomJSONData.CustomBeatmap.CustomBeatmapData")
                ?? throw new TypeLoadException("CustomBeatmapData");
            CustomEventDataType = AccessTools.TypeByName("CustomJSONData.CustomBeatmap.CustomEventData")
                ?? throw new TypeLoadException("CustomEventData");
            _tracksType = typeof(Dictionary<,>).MakeGenericType(typeof(string), _trackType);
        }

        // Heck binds the ReLoader only for debug practice and its constructor needs debug-only bindings, so an empty one is bound to carry the event
        public static void BindReLoader(DiContainer container) {
            if (!IsInstalled) return;
            var setupData = container.TryResolve<GameplayCoreSceneSetupData>();
            if (setupData == null || !_customBeatmapDataType.IsInstanceOfType(setupData.transformedBeatmapData)) return;
            if ((bool)_debugModeProperty.GetValue(null) && setupData.practiceSettings != null) return;
            container.Bind(_reLoaderType).FromInstance(FormatterServices.GetUninitializedObject(_reLoaderType)).AsSingle();
        }

        public static bool NotifyRewind(DiContainer container) {
            if (!IsInstalled || container.TryResolve(_reLoaderType) is not { } reLoader) return false;
            if (container.TryResolve(_tracksType) is IDictionary tracks) {
                foreach (var track in tracks.Values) {
                    _nullPropertiesMethod.Invoke(track, null);
                }
            }
            (_rewindedField.GetValue(reLoader) as Action)?.Invoke();
            return true;
        }
    }
}
