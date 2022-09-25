using BeatLeader.Replayer;
using HarmonyLib;

namespace BeatLeader
{
    [HarmonyPatch(typeof(StandardLevelGameplayManager), "Update")]
    internal static class GameplayManagerPatch
    {
        private static bool Prefix()
        {
            return !ReplayerLauncher.IsStartedAsReplay;
        }
    }
}
