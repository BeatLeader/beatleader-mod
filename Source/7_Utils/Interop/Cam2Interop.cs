using BeatLeader.Replayer;
using System;
using System.Timers;
using System.Reflection;
using UnityEngine;
using BeatLeader.Attributes;
using BeatLeader.Utils;

namespace BeatLeader.Interop {
    [PluginInterop("Camera2")]
    internal static class Cam2Interop {
        [PluginAssembly] 
        private static readonly Assembly _pluginAssembly;

        [PluginType("Camera2.SDK.ReplaySources")] 
        private static readonly Type _replaySourcesType;

        [PluginState]
        public static bool IsInitialized { get; private set; }

        private static object[] _cachedArgs;
        private static Timer _updater;
        private static Transform _headTranform;

        private static Type _genericSourceType;
        private static MethodInfo _registerMethod;
        private static MethodInfo _updateMethod;
        private static MethodInfo _setActiveMethod;
        private static object _genericSourceInstance;

        [InteropEntry]
        private static void Init() {
            _genericSourceType = _replaySourcesType.GetNestedType("GenericSource");
            _setActiveMethod = _genericSourceType.GetMethod("SetActive", ReflectionUtils.DefaultFlags);
            _updateMethod = _genericSourceType.GetMethod("Update", ReflectionUtils.DefaultFlags);
            _registerMethod = _pluginAssembly.GetType("Camera2.SDK.ReplaySources")
               .GetMethod("Register", ReflectionUtils.StaticFlags);

            _genericSourceInstance = CreateGenericSourceInstance("BeatLeaderReplayer");
            RegisterSource(_genericSourceInstance);

            _cachedArgs = new object[2];
            _updater = new(1);
            _updater.Elapsed += OnRepeaterUpdated;

            ReplayerLauncher.ReplayWasStartedEvent += OnReplayWasStarted;
            ReplayerLauncher.ReplayWasFinishedEvent += OnReplayWasFinished;
        }
        public static void SetHeadTransform(Transform transform) {
            _headTranform = transform;
        }
        public static void SetReplayState(bool state) {
            _setActiveMethod?.Invoke(_genericSourceInstance, new object[] { state });
            _updater.Enabled = state;
        }

        private static object CreateGenericSourceInstance(string name) {
            return Activator.CreateInstance(_genericSourceType, new object[] { name });
        }
        private static void UpdateGenericSource() {
            if (_headTranform == null) return;

            var pos = _headTranform.localPosition;
            var rot = _headTranform.localRotation;

            ref var posRef = ref pos;
            ref var rotRef = ref rot;

            _cachedArgs[0] = posRef;
            _cachedArgs[1] = rotRef;

            _updateMethod.Invoke(_genericSourceInstance, _cachedArgs);
        }
        private static void RegisterSource(object source) {
            _registerMethod?.Invoke(null, new object[] { source });
        }

        private static void OnRepeaterUpdated(object sender, ElapsedEventArgs e) {
            UpdateGenericSource();
        }
        private static void OnReplayWasStarted(Models.ReplayLaunchData data) => SetReplayState(true);
        private static void OnReplayWasFinished(Models.ReplayLaunchData data) => SetReplayState(false);
    }
}
