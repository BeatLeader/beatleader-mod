using BeatLeader.Replayer;
using IPA.Loader;
using System;
using System.Timers;
using System.Reflection;
using UnityEngine;
using System.Threading.Tasks;

namespace BeatLeader.Interop
{
    internal static class Cam2Interop
    {
        public static bool DetectedAndValid { get; private set; } = false;

        private static Assembly _pluginAssembly;
        private static Type _genericSourceType;
        private static MethodInfo _registerMethod;
        private static MethodInfo _updateMethod;
        private static MethodInfo _setActiveMethod;
        private static object _genericSourceInstance;
        private static object[] _cachedArgs = new object[2];

        private static Timer _updater;
        private static Transform _headTranform;

        public static void Init()
        {
            _pluginAssembly = PluginManager.GetPluginFromId("Camera2")?.Assembly;
            if (_pluginAssembly == null) return;

            try
            {
                ResolveData();
                _genericSourceInstance = CreateGenericSourceInstance("BeatLeaderReplayer");
                RegisterSource(_genericSourceInstance);
            }
            catch
            {
                Plugin.Log.Error("Failed to resolve Camera2 data, replays system may conflict with Camera2!");
                return;
            }

            DetectedAndValid = true;
            _updater = new Timer(1);
            _updater.Elapsed += OnRepeaterUpdated;

            ReplayerLauncher.ReplayWasStartedEvent += OnReplayStarted;
            ReplayerLauncher.ReplayWasFinishedEvent += OnReplayFinished;
        }
        public static void SetHeadTransform(Transform transform)
        {
            _headTranform = transform;
        }
        public static void SetReplayState(bool state)
        {
            if (_setActiveMethod == null) return;

            _setActiveMethod.Invoke(_genericSourceInstance, new object[] { state });
            _updater.Enabled = state;
        }

        private static object CreateGenericSourceInstance(string name)
        {
            return Activator.CreateInstance(_genericSourceType, new object[] { name });
        }
        private static void UpdateGenericSource()
        {
            if (DetectedAndValid && _headTranform == null) return;

            var pos = _headTranform.localPosition;
            var rot = _headTranform.localRotation;

            ref var posRef = ref pos;
            ref var rotRef = ref rot;

            _cachedArgs[0] = posRef;
            _cachedArgs[1] = rotRef;

            _updateMethod.Invoke(_genericSourceInstance, _cachedArgs);
        }
        private static void RegisterSource(object source)
        {
            _registerMethod?.Invoke(null, new object[] { source });
        }
        private static void ResolveData()
        {
            var replaySourcesType = _pluginAssembly.GetType("Camera2.SDK.ReplaySources");
            _genericSourceType = replaySourcesType.GetNestedType("GenericSource");

            _setActiveMethod = _genericSourceType.GetMethod("SetActive",
                BindingFlags.Instance | BindingFlags.Public);

            _updateMethod = _genericSourceType.GetMethod("Update",
                BindingFlags.Instance | BindingFlags.Public);

            _registerMethod = _pluginAssembly.GetType("Camera2.SDK.ReplaySources")
               .GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
        }

        private static void OnRepeaterUpdated(object sender, ElapsedEventArgs e)
        {
            UpdateGenericSource();
        }
        private static void OnReplayStarted(Models.ReplayLaunchData data) => SetReplayState(true);
        private static void OnReplayFinished(Models.ReplayLaunchData data) => SetReplayState(false);
    }
}
