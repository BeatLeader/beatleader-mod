using System;
using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(GameplayCoreInstaller), "InstallBindings")]
    public static class GameInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(GameplayCoreInstaller __instance) {
            try {
                OnGameplayCoreInstaller.Install(__instance.Container);
            } catch (Exception ex) {
                Plugin.Log.Critical($"---\nGameInstaller exception: {ex.Message}\n{ex.StackTrace}\n---");
            }
        }
    }
}