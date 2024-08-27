using System;
using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(PCAppInit), "InstallBindings")]
    public static class AppInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(PCAppInit __instance) {
            try {
                BLLocalization.Initialize(__instance._mainSystemInit._mainSettingsModel);
                OnAppInitInstaller.Install(__instance.Container);
            } catch (Exception ex) {
                Plugin.Log.Critical($"---\nAppInstaller exception: {ex.Message}\n{ex.StackTrace}\n---");
            }
        }
    }
}