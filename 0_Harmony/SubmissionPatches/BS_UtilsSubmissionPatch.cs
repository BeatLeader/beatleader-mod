using BeatLeader.Utils;
using HarmonyLib;
using IPA.Loader;

namespace BeatLeader {
    internal class BS_UtilsSubmissionPatch {

        private static readonly string _pluginId = "BS Utils";

        public static void ApplyPatch(Harmony harmony) {
            var type = PluginManager.GetPluginFromId(_pluginId)?.Assembly?.GetType("BS_Utils.Gameplay.ScoreSubmission");
            if (type != null) {
                var mOriginal = AccessTools.Method(type, "DisableScoreSaberScoreSubmission");
                var mPrefix = SymbolExtensions.GetMethodInfo(() => MyPrefix());

                harmony.Patch(mOriginal, new HarmonyMethod(mPrefix));
            }
        }

        public static void MyPrefix() {
            ScoreUtil.BS_UtilsSubmission = false;
        }
    }
}
