using System;
using BeatLeader.Installers;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(MainSettingsMenuViewControllersInstaller), "InstallBindings")]
    public static class MenuInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MainSettingsMenuViewControllersInstaller __instance) {
            try {
                var container = __instance.GetContainer();
                OnMenuInstaller.Install(container);
            } catch (Exception ex) {
                Plugin.Log.Critical($"---\nMenuInstaller exception: {ex.Message}\n{ex.StackTrace}\n---");
            }
        }
    }
}