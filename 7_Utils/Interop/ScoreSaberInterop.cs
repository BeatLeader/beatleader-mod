using BeatLeader.Utils;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zenject;

namespace BeatLeader.Interop
{
    internal static class ScoreSaberInterop
    {
        private static Assembly _assembly;
        private static List<MethodInfo> _installersMethods;
        private static HarmonyMultisilencer _installatorsSilencer;

        public static bool RecordingEnabled
        {
            get => !_installatorsSilencer?.Enabled ?? true;
            set
            {
                if (_installatorsSilencer != null)
                    _installatorsSilencer.Enabled = !value;
            }
        }

        public static void Init()
        {
            _assembly = PluginManager.GetPluginFromId("ScoreSaber")?.Assembly;
            if (_assembly == null) return;

            try
            {
                ResolveData();
                _installatorsSilencer = new(_installersMethods, false);
                Plugin.Log.Info($"Successfully patched {_installersMethods.Count} ScoreSaber installators!");
            }
            catch
            {
                Plugin.Log.Error("Failed to resolve ScoreSaber data, replays system may conflict with ScoreSaber!");
            }
        }
        private static void ResolveData()
        {
            var types = _assembly.GetTypes();
            _installersMethods = new List<MethodInfo>();

            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(Installer)))
                {
                    var method = item.GetMethod(nameof(
                        Installer.InstallBindings), ReflectionUtils.DefaultFlags);
                    _installersMethods.Add(method);
                }
            }
        }
    }
}
