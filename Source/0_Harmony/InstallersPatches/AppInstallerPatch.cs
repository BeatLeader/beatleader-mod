using System;
using BeatLeader.Installers;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(PCAppInit), "InstallBindings")]
    public static class AppInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(PCAppInit __instance) {
                var container = __instance.GetContainer();
            try {
                OnAppInitInstaller.Install(container);
            } catch (Exception ex) {
                Plugin.Log.Critical($"---\nAppInstaller exception: {ex.Message}\n{ex.StackTrace}\n---");
            }
        }
    }
}