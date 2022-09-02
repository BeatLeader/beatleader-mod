using BeatLeader.Replayer;
using IPA.Loader;
using System;
using System.Reflection;
using UnityEngine;
using System.Threading;

namespace BeatLeader.Interop
{
    internal static class Cam2Interop
    {
        public static bool Detected { get; private set; } = false;

        private static Assembly _pluginAssembly;
        private static Type _genericSourceType;
        private static MethodInfo _registerMethod;
        private static MethodInfo _updateMethod;
        private static object _genericSourceInstance;
        private static object[] _cachedArgs = new object[3];

        private static Timer _updater;
        private static Transform _headTranform;
        private static bool _isPlaying;

        public static void Init()
        {
            _pluginAssembly = PluginManager.GetPluginFromId("Camera2")?.Assembly;
            if (_pluginAssembly == null) return;
            Detected = true;

            ResolveData();
            _genericSourceInstance = CreateGenericSourceInstance("BeatLeaderReplayer");
            RegisterSource(_genericSourceInstance);

            ReplayerLauncher.OnReplayStart += NotifyReplayStarted;
            ReplayerLauncher.OnReplayFinish += NotifyReplayFinished;
        }
        public static void SetHeadTransform(Transform transform)
        {
            _headTranform = transform;
        }
        public static void SetReplayState(bool state)
        {
            _isPlaying = state;
            if (state)
                _updater = new Timer(NotifyRepeaterUpdated, null, 0, 1);
            else
                _updater.Dispose();
        }

        private static object CreateGenericSourceInstance(string name)
        {
            return Activator.CreateInstance(_genericSourceType, new object[] { name });
        }
        private static void UpdateGenericSource()
        {
            _cachedArgs[0] = _isPlaying;

            var pos = _headTranform != null ? _headTranform.localPosition : Vector3.zero;
            var rot = _headTranform != null ? _headTranform.localRotation : Quaternion.identity;
            ref var posRef = ref pos;
            ref var rotRef = ref rot;

            _cachedArgs[1] = posRef;
            _cachedArgs[2] = rotRef;

            _updateMethod.Invoke(_genericSourceInstance, _cachedArgs);
        }
        private static void RegisterSource(object source)
        {
            _registerMethod.Invoke(null, new object[] { source });
        }
        private static void ResolveData()
        {
            var replaySourcesType = _pluginAssembly.GetType("Camera2.SDK.ReplaySources");
            _genericSourceType = replaySourcesType.GetNestedType("GenericSource");
            _updateMethod = _genericSourceType.GetMethod("Update",
                BindingFlags.Instance | BindingFlags.Public);
            _registerMethod = _pluginAssembly.GetType("Camera2.SDK.ReplaySources")
               .GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
        }

        private static void NotifyRepeaterUpdated(object state)
        {
            UpdateGenericSource();
        }
        private static void NotifyReplayStarted(Models.ReplayLaunchData data) => SetReplayState(true);
        private static void NotifyReplayFinished(Models.ReplayLaunchData data) => SetReplayState(false);
    }
}
