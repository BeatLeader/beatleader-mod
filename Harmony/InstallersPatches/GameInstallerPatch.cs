using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;
using BeatLeader.Utils;

namespace BeatLeader 
{
    [HarmonyPatch(typeof(GameplayCoreInstaller), "InstallBindings")]
    public static class GameInstallerPatch 
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Prefix(GameplayCoreInstaller __instance)
        {
            Zenjector.gameplayCoreContainer = __instance.GetContainer();
            Zenjector.InvokeEvent(Zenjector.Location.GameplayCore, Zenjector.Time.Before);
        }
        private static void Postfix(GameplayCoreInstaller __instance) 
        {
            Zenjector.InvokeEvent(Zenjector.Location.GameplayCore, Zenjector.Time.After);
        }
    }
}