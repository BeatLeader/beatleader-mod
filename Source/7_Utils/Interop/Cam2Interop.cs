using BeatLeader.Replayer;
using System;
using System.Reflection;
using UnityEngine;
using BeatLeader.Attributes;
using BeatLeader.Utils;

namespace BeatLeader.Interop {
    [PluginInterop("Camera2")]
    internal static class Cam2Interop {
        [PluginAssembly]
        private static readonly Assembly pluginAssembly = null!;

        [PluginType("Camera2.SDK.ReplaySources")]
        private static readonly Type replaySourcesType = null!;

        [PluginState]
        public static bool IsInitialized { get; private set; }
        public static Transform? HeadTransform { set => _headTransform = value; }
        private static bool ReplayState {
            set {
                _cachedArgs[0] = value;
                _setActiveMethod?.Invoke(_genericSourceInstance, _cachedArgs);
            }
        }

        private static HarmonyAutoPatch? _headPositionPropertyPatch;
        private static HarmonyAutoPatch? _headRotationPropertyPatch;

        private static object[] _cachedArgs = null!;
        private static Transform? _headTransform;
        private static PropertyInfo? _headPosProp;
        private static PropertyInfo? _headRotProp;
        private static MethodInfo? _setActiveMethod;
        private static object? _genericSourceInstance;

        [InteropEntry]
        private static void Init() {
            var genericSourceType = replaySourcesType.GetNestedType("GenericSource");
            var registerMethod = replaySourcesType.GetMethod("Register", ReflectionUtils.StaticFlags);
            _setActiveMethod = genericSourceType.GetMethod("SetActive", ReflectionUtils.DefaultFlags);

            _genericSourceInstance = Activator.CreateInstance(genericSourceType, "BeatLeaderReplayer");
            registerMethod?.Invoke(null, new[] { _genericSourceInstance });

            _cachedArgs = new object[1];

            _headPosProp = genericSourceType.GetProperty(
                "localHeadPosition", ReflectionUtils.DefaultFlags);

            _headRotProp = genericSourceType.GetProperty(
                "localHeadRotation", ReflectionUtils.DefaultFlags);
            
            _headPositionPropertyPatch = new(new(
                _headPosProp!.GetGetMethod(), typeof(Cam2Interop)
                    .GetMethod(nameof(LocalHeadPositionPrefix), ReflectionUtils.StaticFlags)));

            _headRotationPropertyPatch = new(new(
                _headRotProp!.GetGetMethod(), typeof(Cam2Interop)
                    .GetMethod(nameof(LocalHeadRotationPrefix), ReflectionUtils.StaticFlags)));

            ReplayerLauncher.ReplayWasStartedEvent += HandleReplayWasStarted;
            ReplayerLauncher.ReplayWasFinishedEvent += HandleReplayWasFinished;
        }

        // ReSharper disable once InconsistentNaming
        private static void LocalHeadPositionPrefix(object __instance) {
            if (_genericSourceInstance != __instance || _headTransform == null) return;
            _headPosProp?.SetValue(__instance, _headTransform.localPosition);
        }

        // ReSharper disable once InconsistentNaming
        private static void LocalHeadRotationPrefix(object __instance) {
            if (_genericSourceInstance != __instance || _headTransform == null) return;
            _headRotProp?.SetValue(__instance, _headTransform.localRotation);
        }
        
        private static void HandleReplayWasStarted(Models.ReplayLaunchData data) {
            if (InputUtils.IsInFPFC) ReplayerLauncher.LaunchData!.Settings.CameraSettings = null; //disabling base camera
            ReplayState = true;
        }

        private static void HandleReplayWasFinished(Models.ReplayLaunchData data) => ReplayState = false;
    }
}