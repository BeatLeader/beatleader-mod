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
        private static Type _replaySourceType;
        private static FieldInfo _nameField;
        private static FieldInfo _isPlayingField;
        private static FieldInfo _replayHeadTransformField;
        private static FieldInfo _offsetField;
        private static MethodInfo _registerMethod;
        private static object _detectorSourceInstance;

        public static void Init()
        {
            _pluginAssembly = PluginManager.GetPluginFromId("Camera2")?.Assembly;
            if (_pluginAssembly == null) return;
            Detected = true;

            CreateDetectorSourceType();
            ResolveFields();
            _detectorSourceInstance = CreateDetectorSourceInstance("BeatLeaderReplayer");
            RegisterDetectorSource(_detectorSourceInstance);

            ReplayerLauncher.OnReplayStart += OnReplayStarted;
            ReplayerLauncher.OnReplayFinish += OnReplayFinished;
        }
        public static void ApplyHeadTransform(Transform transform)
        {
            _replayHeadTransformField?.SetValue(_detectorSourceInstance, transform);
        }
        public static void ApplyReplayState(bool state)
        {
            _isPlayingField?.SetValue(_detectorSourceInstance, state);
        }

        private static object CreateDetectorSourceInstance(string name)
        {
            var instance = Activator.CreateInstance(_replaySourceType);
            _nameField.SetValue(instance, name);
            return instance;
        }
        private static void RegisterDetectorSource(object detector)
        {
            _registerMethod.Invoke(null, new object[] { detector });
        }
        private static void CreateDetectorSourceType()
        {
            var moduleBuilder = ReflectionUtils.CreateModuleBuilder("BL_Cam2_Builder");
            var replaySourcesType = _pluginAssembly.GetType("Camera2.SDK.ReplaySources");
            var sourceInterfaceType = replaySourcesType.GetNestedType("ISource");
            var type = moduleBuilder.DefineType("BeatLeaderDetectorSource", TypeAttributes.Public | TypeAttributes.Class,
                null, new[] { sourceInterfaceType });

            type.AddDefaultConstructor();

            var nameField = type.DefineField("_name", typeof(string), FieldAttributes.Private);
            type.AddGetOnlyProperty("name", nameField, sourceInterfaceType.GetMethod("get_name"));
            
            var isPlayingField = type.DefineField("_isPlaying", typeof(bool), FieldAttributes.Private);
            type.AddGetOnlyProperty("isPlaying", isPlayingField, sourceInterfaceType.GetMethod("get_isPlaying"));
            
            var replayHeadTransformField = type.DefineField("_replayHeadTransform", typeof(Transform), FieldAttributes.Private);
            type.AddGetOnlyProperty("replayHeadTransform", replayHeadTransformField, sourceInterfaceType.GetMethod("get_replayHeadTransform"));

            var offsetGetter = sourceInterfaceType.GetMethod("get_offset");
            if (offsetGetter != null)
            {
                var offsetField = type.DefineField("_offset", typeof(Transform), FieldAttributes.Private);
                type.AddGetOnlyProperty("offset", offsetField, offsetGetter);
            }

            _replaySourceType = type.CreateType();
        }
        private static void ResolveFields()
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            _nameField = _replaySourceType.GetField("_name", flags);
            _isPlayingField = _replaySourceType.GetField("_isPlaying", flags);
            _replayHeadTransformField = _replaySourceType.GetField("_replayHeadTransform", flags);
            _offsetField = _replaySourceType.GetField("_offset", flags);
            _registerMethod = _pluginAssembly.GetType("Camera2.SDK.ReplaySources")
                .GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
        }

        private static void OnReplayStarted(Models.ReplayLaunchData data) => ApplyReplayState(true);
        private static void OnReplayFinished(Models.ReplayLaunchData data) => ApplyReplayState(false);
    }
}
