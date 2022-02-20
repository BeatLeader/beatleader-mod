using System.Reflection;
using HarmonyLib;

namespace BeatLeader {
    public static class HarmonyHelper {
        private static Harmony _harmony;

        private static bool _initialized;

        private static void LazyInit() {
            if (_initialized) return;
            _harmony = new Harmony(Plugin.HarmonyId);
            _initialized = true;
        }

        public static void ApplyPatches() {
            LazyInit();
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void RemovePatches() {
            if (!_initialized) return;
            _harmony.UnpatchSelf();
        }
    }
}