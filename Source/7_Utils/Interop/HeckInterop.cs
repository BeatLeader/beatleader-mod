using System;
using System.Linq;
using System.Reflection;
using BeatLeader.Attributes;
using BeatLeader.Utils;

namespace BeatLeader.Interop {
    [PluginInterop("_Heck")]
    internal static class HeckInterop {
        [PluginState]
        public static bool IsInstalled { get; private set; }

        [PluginAssembly]
        public static Assembly? Assembly { get; private set; }

        [PluginType("Heck.PlayView.IPlayViewController")]
        public static Type? PlayViewControllerType { get; private set; }

        [PluginType("Heck.SettingsSetter.SettingsSetterViewController")]
        public static Type? SetterViewControllerType { get; private set; }

        [PluginType("Heck.PlayView.StartStandardLevelParameters")]
        private static Type _startParametersType = null!;

        private static ConstructorInfo _startParametersConstructor = null!;

        [InteropEntry]
        private static void Init() {
            _startParametersConstructor = _startParametersType
                .GetConstructors()
                .First(x => x.GetParameters().Length > 1);
        }

        public static object CreateStartData(
            string gameMode,
            in BeatmapKey beatmapKey,
            BeatmapLevel beatmapLevel,
            OverrideEnvironmentSettings? overrideEnvironmentSettings,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings
        ) {
            if (!IsInstalled) {
                throw new InvalidOperationException("Heck is not installed");
            }
            return _startParametersConstructor.Invoke(
                new object?[] {
                    gameMode,
                    beatmapKey,
                    beatmapLevel,
                    overrideEnvironmentSettings,
                    null,
                    null,
                    gameplayModifiers,
                    playerSpecificSettings,
                    null,
                    null,
                    string.Empty,
                    false,
                    false,
                    null,
                    null,
                    null,
                    null,
                    null
                }
            );
        }
    }
}