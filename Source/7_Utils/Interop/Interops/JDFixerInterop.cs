using BeatLeader.Attributes;
using BeatLeader.Utils;
using System;
using System.Reflection;

namespace BeatLeader.Interop {
    [PluginInterop("JDFixer")]
    internal static class JDFixerInterop {
        [PluginType("JDFixer.PluginConfig")]
        private static readonly Type _pluginConfigType = null!;

        private static PropertyInfo _pluginConfigInstanceProperty = null!;
        private static PropertyInfo _pluginConfigEnabledProperty = null!;

        [PluginState]
        public static bool IsInitialized { get; private set; }

        public static bool Enabled {
            get => IsInitialized && _pluginConfigEnabledProperty.GetValue(PluginConfig) != null;
            set {
                if (!IsInitialized) return;
                _pluginConfigEnabledProperty.SetValue(PluginConfig, value);
                Plugin.Log.Info("[JDFixerInterop] JD patch state: " + (value ? "active" : "inactive"));
            }
        }

        private static object? PluginConfig {
            get => _pluginConfigInstanceProperty.GetValue(null);
        }

        [InteropEntry]
        private static void Init() {
            _pluginConfigInstanceProperty =_pluginConfigType
                .GetProperty("Instance", ReflectionUtils.StaticFlags)!;
            _pluginConfigEnabledProperty = _pluginConfigType
                .GetProperty("enabled", ReflectionUtils.DefaultFlags)!;
            if (_pluginConfigInstanceProperty == null)
                throw new MissingMemberException("Property Instance of type JDFixer.PluginConfig does not exist!");
            if (_pluginConfigEnabledProperty == null)
                throw new MissingMemberException("Property enabled of type bool does not exist!");
        }
    }
}
