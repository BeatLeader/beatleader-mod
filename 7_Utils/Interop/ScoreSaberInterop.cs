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
                var types = _assembly.GetTypes();
                var methods = new List<MethodInfo>();

                foreach (var item in types)
                {
                    if (item.IsSubclassOf(typeof(Installer)))
                    {
                        var method = item.GetMethod(nameof(
                            Installer.InstallBindings), ReflectionUtils.DefaultFlags);
                        methods.Add(method);
                    }
                }

                Plugin.Log.Info($"Found {methods.Count} ScoreSaber installators");
                _installatorsSilencer = new(methods, false);
            }
            catch
            {
                Plugin.Log.Error("Failed to resolve ScoreSaber data, replays system may conflict with ScoreSaber!");
            }
        }
    }
}
