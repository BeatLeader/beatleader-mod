using BeatLeader.UI.Reactive.Yoga;

namespace BeatLeader.UI.Reactive {
    internal static class ReactivePlatform {
        public static void Init() {
            if (YogaNative.Load()) {
                Plugin.Log.Debug("[ReactivePlatform] Yoga engine loaded successfully");
            } else {
                Plugin.Log.Error("[ReactivePlatform] Yoga engine failed to load!");
            }
        }
    }
}