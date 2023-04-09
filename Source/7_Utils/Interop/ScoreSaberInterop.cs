using BeatLeader.Attributes;
using BeatLeader.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.Replayer;
using Zenject;

namespace BeatLeader.Interop {
    [PluginInterop("ScoreSaber")]
    internal static class ScoreSaberInterop {
        [PluginAssembly]
        private static readonly Assembly assembly = null!;
        private static HarmonyMultisilencer? _installatorsSilencer;

        private static bool RecordingEnabled {
            set {
                if (_installatorsSilencer != null)
                    _installatorsSilencer.Enabled = !value;
            }
        }

        [InteropEntry]
        private static void Init() {
            var methods = assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(Installer)))
                .Select(x => x.GetMethod(nameof(Installer
                    .InstallBindings), ReflectionUtils.DefaultFlags))
                .ToArray();

            _installatorsSilencer = new(methods, false);
            Plugin.Log.Info($"Successfully patched {methods.Length} ScoreSaber installators!");

            ReplayerLauncher.ReplayWasStartedEvent += HandleReplayWasStarted;
            ReplayerLauncher.ReplayWasFinishedEvent += HandleReplayWasFinished;
        }

        private static void HandleReplayWasStarted(Models.ReplayLaunchData data) => RecordingEnabled = false;
        private static void HandleReplayWasFinished(Models.ReplayLaunchData data) => RecordingEnabled = true;
    }
}