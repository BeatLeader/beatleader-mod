using BeatLeader.Replayer;
using BeatLeader.Utils;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        private static Repeater _updater;
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

            _updater = Repeater.Create(NotifyRepeaterUpdated, false);
        }
        public static void SetHeadTransform(Transform transform)
        {
            _headTranform = transform;
        }
        public static void SetReplayState(bool state)
        {
            _isPlaying = state;
            if (state) _updater.Run();
            else _updater.Stop();
        }

        private static object CreateGenericSourceInstance(string name)
        {
            return Activator.CreateInstance(_genericSourceType, new object[] { name });
        }
        private static void UpdateGenericSource()
        {
            _updateMethod.Invoke(_genericSourceInstance, new object[] 
            {
                _isPlaying,
                _headTranform != null ? _headTranform.localPosition : default(Vector3),
                _headTranform != null ? _headTranform.localRotation : default(Quaternion)
            });
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

        private static void NotifyRepeaterUpdated()
        {
            UpdateGenericSource();
        }
        private static void NotifyReplayStarted(Models.ReplayLaunchData data) => SetReplayState(true);
        private static void NotifyReplayFinished(Models.ReplayLaunchData data) => SetReplayState(false);
    }
}
