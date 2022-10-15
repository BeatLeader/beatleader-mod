using BeatLeader.Attributes;
using BeatLeader.Utils;
using System.Collections.Generic;
using System.Reflection;
using Zenject;

namespace BeatLeader.Interop {
    [PluginInterop("ScoreSaber")]
    internal static class ScoreSaberInterop {
        [PluginAssembly] private static readonly Assembly _assembly;

        private static List<MethodInfo> _installersMethods;
        private static HarmonyMultisilencer _installatorsSilencer;

        public static bool RecordingEnabled {
            get => !_installatorsSilencer?.Enabled ?? true;
            set {
                if (_installatorsSilencer != null)
                    _installatorsSilencer.Enabled = !value;
            }
        }

        [InteropEntry]
        private static void Init() {
            var types = _assembly.GetTypes();
            _installersMethods = new List<MethodInfo>();

            foreach (var item in types) {
                if (item.IsSubclassOf(typeof(Installer))) {
                    var method = item.GetMethod(nameof(
                        Installer.InstallBindings), ReflectionUtils.DefaultFlags);
                    _installersMethods.Add(method);
                }
            }

            _installatorsSilencer = new(_installersMethods, false);
            Plugin.Log.Info($"Successfully patched {_installersMethods.Count} ScoreSaber installators!");
        }
    }
}
