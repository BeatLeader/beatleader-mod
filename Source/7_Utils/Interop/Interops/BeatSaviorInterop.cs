using BeatLeader.Attributes;
using BeatLeader.Utils;
using HarmonyLib;
using System;
using System.Reflection;
using BeatLeader.Replayer;

namespace BeatLeader.Interop {
    [PluginInterop("BeatSaviorData")]
    internal static class BeatSaviorInterop {
        [PluginAssembly]
        private static readonly Assembly assembly = null!;

        [PluginType("BeatSaviorData.SettingsMenu")]
        private static readonly Type settingsMenuType = null!;

        [PluginType("BeatSaviorData.Plugin")]
        private static readonly Type pluginType = null!;

        private static HarmonyAutoPatch? _uploadScoreMethodPatch;

        private static object[] _cachedArgs = null!;
        private static MethodInfo? _setBoolMethod;
        private static object? _configInstance;
        private static bool _isMarkedToEnable;

        private static bool SubmissionEnabled {
            set {
                _cachedArgs[2] = value;
                _setBoolMethod?.Invoke(_configInstance, _cachedArgs);
            }
        }

        [InteropEntry]
        private static void Init() {
            _configInstance = settingsMenuType.GetField("config",
                ReflectionUtils.StaticFlags)!.GetValue(null);
            _cachedArgs = new object[] { "BeatSaviorData", "DisableBeatSaviorUpload", false };

            _setBoolMethod = _configInstance.GetType().GetMethod("SetBool",
                new[] { typeof(string), typeof(string), typeof(bool) });

            _uploadScoreMethodPatch = new(new(
                pluginType.GetMethod("UploadData",
                    ReflectionUtils.DefaultFlags)!, postfix:
                typeof(BeatSaviorInterop).GetMethod(nameof(
                    UploadScorePostfix), ReflectionUtils.StaticFlags)));
            
            ReplayerLauncher.ReplayWasStartedEvent += HandleReplayWasStarted;
            ReplayerLauncher.ReplayWasFinishedEvent += HandleReplayWasFinished;
        }

        private static void UploadScorePostfix() {
            if (!_isMarkedToEnable) return;
            SubmissionEnabled = true;
            _isMarkedToEnable = false;
        }

        private static void HandleReplayWasStarted(Models.ReplayLaunchData data) => SubmissionEnabled = false;
        private static void HandleReplayWasFinished(Models.ReplayLaunchData data) => _isMarkedToEnable = true;
    }
}