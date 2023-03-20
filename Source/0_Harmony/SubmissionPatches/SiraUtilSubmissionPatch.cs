using BeatLeader.Utils;
using HarmonyLib;
using IPA.Loader;

namespace BeatLeader {
    internal class SiraUtilSubmissionPatch {

        private static readonly string _pluginId = "SiraUtil";

        public static void ApplyPatch(Harmony harmony) {
            var type = PluginManager.GetPluginFromId(_pluginId)?.Assembly?.GetType("SiraUtil.Submissions.SubmissionDataContainer");
            if (type != null) {
                var mOriginal = AccessTools.Method(type, "SSS");
                var mPrefix = SymbolExtensions.GetMethodInfo<bool>((value) => MyPrefix(value));

                harmony.Patch(mOriginal, new HarmonyMethod(mPrefix));
            }
        }

        public static void MyPrefix(bool value) {
            ScoreUtil.SiraSubmission = value;
        }
    }
}
