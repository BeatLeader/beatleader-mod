using BeatLeader.Installers;
using BGLib.AppFlow;
using HarmonyLib;
using JetBrains.Annotations;
using System;

namespace BeatLeader {
    [HarmonyPatch(typeof(FeatureAsyncInstaller), "InstallBindings")]
    public static class AppInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(FeatureAsyncInstaller __instance) {
            try {
                OnAppInitInstaller.Install(__instance.Container);
            } catch (Exception ex) {
                Plugin.Log.Critical($"---\nAppInstaller exception: {ex.Message}\n{ex.StackTrace}\n---");
            }
        }
    }
}