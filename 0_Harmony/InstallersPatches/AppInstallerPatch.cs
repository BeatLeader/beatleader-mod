using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(PCAppInit), "InstallBindings")]
    public static class AppInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(PCAppInit __instance) {
            var container = __instance.GetContainer();
            OnAppInitInstaller.Install(container);
        }
    }
}