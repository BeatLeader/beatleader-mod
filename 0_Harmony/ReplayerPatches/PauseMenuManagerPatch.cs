using BeatLeader.Replayer;
using HarmonyLib;

namespace BeatLeader
{
    [HarmonyPatch(typeof(PauseMenuManager), "Update")]
    internal static class PauseMenuManagerPatch
    {
        private static bool Prefix()
        {
            return !ReplayerLauncher.IsStartedAsReplay;
        }
    }
}
