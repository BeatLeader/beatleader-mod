using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(GameplayCoreInstaller), "InstallBindings")]
    public static class GameInstallerPatch {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(GameplayCoreInstaller __instance) {
            var container = __instance.GetContainer();
            OnGameplayCoreInstaller.Install(container);
        }
    }
}