using BeatLeader.Installers;
using HarmonyLib;
using JetBrains.Annotations;
using BeatLeader.Utils;

namespace BeatLeader 
{
    [HarmonyPatch(typeof(PCAppInit), "InstallBindings")]
    public static class AppInstallerPatch 
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Prefix(PCAppInit __instance)
        {
            Zenjector.appContainer = __instance.GetContainer();
            Zenjector.InvokeEvent(Zenjector.Location.App, Zenjector.Time.Before);
        }
        private static void Postfix(PCAppInit __instance) 
        {
            Zenjector.InvokeEvent(Zenjector.Location.App, Zenjector.Time.After);
        }
    }
}