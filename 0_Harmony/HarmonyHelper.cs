using System;
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
            PatchScoreSubmission();
            PatchCustomCampaigns();
            PatchTournamentAssistant();
        }

        private static void PatchScoreSubmission() {
            try {
                SiraUtilSubmissionPatch.ApplyPatch(_harmony);
                BS_UtilsSubmissionPatch.ApplyPatch(_harmony);
            } catch (Exception e) {
                Plugin.Log.Warn("Can't patch score disable methods");
                Plugin.Log.Warn(e);
            }
        }

        private static void PatchCustomCampaigns() {
            try {
                RecorderCustomCampaignUtilPatch.ApplyPatch(_harmony);
            } catch (Exception e) {
                Plugin.Log.Warn("Can't patch Custom Campaigns methods");
                Plugin.Log.Warn(e);
            }
        }

        private static void PatchTournamentAssistant() {
            try {
                RecorderTournamentAssistantUtilPatch.ApplyPatch(_harmony);
            } catch (Exception e) {
                Plugin.Log.Warn("Can't patch Tournament Assistant methods");
                Plugin.Log.Warn(e);
            }
        }

        public static void RemovePatches() {
            if (!_initialized) return;
            _harmony.UnpatchSelf();
        }
    }
}