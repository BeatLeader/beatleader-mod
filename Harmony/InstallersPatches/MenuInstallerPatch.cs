using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;
using BeatLeader.Utils;

namespace BeatLeader 
{
    [HarmonyPatch(typeof(MainSettingsMenuViewControllersInstaller), "InstallBindings")]
    public static class MenuInstallerPatch 
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Prefix(MainSettingsMenuViewControllersInstaller __instance)
        {
            Zenjector.menuContainer = __instance.GetContainer();
            Zenjector.InvokeEvent(Zenjector.Location.Menu, Zenjector.Time.Before);
        }
        private static void Postfix(MainSettingsMenuViewControllersInstaller __instance) 
        {
            Zenjector.InvokeEvent(Zenjector.Location.Menu, Zenjector.Time.After);
        }

    }
}