using System;
using System.Linq;
using BeatLeader.Models;

namespace BeatLeader.DataManager {
    public static class LeaderboardContextSwitcher {
        private static bool _initialized;
        private static string? _modifiersContextKey;
        private static bool _switching;

        public static void Initialize() {
            PluginConfig.ScoresContextChangedEvent -= HandleScoresContextChanged;
            PluginConfig.ScoresContextChangedEvent += HandleScoresContextChanged;
        }

        public static void HandleModifiersChanged(GameplayModifiers modifiers) {
            var key = GetContextKey(modifiers);
            if (_initialized && key == _modifiersContextKey) return;
            var keepSessionStartContext = !_initialized && ConfigFileData.Instance.ManualScoresContext == null;
            _initialized = true;
            _modifiersContextKey = key;
            if (!keepSessionStartContext) SwitchTo(key);
        }

        private static string? GetContextKey(GameplayModifiers modifiers) {
            if (IsEnabled(GolfModifier.Id)) return "golf";
            if (IsEnabled(DanceModifier.Id)) return "dnc";
            if (IsEnabled(SpeedModifiers.BSF.Id)) return "bsf";
            if (IsEnabled(SpeedModifiers.BFS.Id)) return "bfs";
            return modifiers.songSpeed switch {
                GameplayModifiers.SongSpeed.Slower => "ss",
                GameplayModifiers.SongSpeed.Faster => "fs",
                GameplayModifiers.SongSpeed.SuperFast => "sf",
                _ => null
            };
        }

        private static bool IsEnabled(string id) {
            return ModifiersCore.ModifiersManager.HasModifierWithId(id) && ModifiersCore.ModifiersManager.GetModifierState(id);
        }

        private static void SwitchTo(string? key) {
            var config = ConfigFileData.Instance;
            if (key == null) {
                if (config.ManualScoresContext is not { } manualContext) return;
                config.ManualScoresContext = null;
                SetScoresContext(manualContext);
                return;
            }
            var context = ScoresContexts.AllContexts.FirstOrDefault(x => x.Key == key);
            if (context == null) return;
            config.ManualScoresContext ??= PluginConfig.ScoresContext;
            SetScoresContext(context.Id);
        }

        private static void SetScoresContext(int id) {
            _switching = true;
            try {
                PluginConfig.ScoresContext = id;
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to switch the scores context: {ex}");
            } finally {
                _switching = false;
            }
        }

        private static void HandleScoresContextChanged(int id) {
            if (_switching) return;
            ConfigFileData.Instance.ManualScoresContext = null;
        }
    }
}
