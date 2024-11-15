using System;
using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(MainSystemInit), "Init")]
    public static class MainSystemInitPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MainSystemInit __instance) {
            try {
                BLLocalization.Initialize(__instance._mainSettingsHandler);
            } catch (Exception ex) {
                Plugin.Log.Critical($"---\nMainSystemInit patch exception: {ex.Message}\n{ex.StackTrace}\n---");
            }
        }
    }
}