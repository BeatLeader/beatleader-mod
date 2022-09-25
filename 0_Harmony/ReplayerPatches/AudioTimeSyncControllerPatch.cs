using BeatLeader.Replayer;
using HarmonyLib;

namespace BeatLeader
{
    [HarmonyPatch(typeof(AudioTimeSyncController), "Update")]
    internal static class AudioTimeSyncControllerPatch
    {
        private static bool Prefix(AudioTimeSyncController __instance)
        {
            return !(ReplayerLauncher.IsStartedAsReplay && __instance.state.Equals(AudioTimeSyncController.State.Paused));
        }
    }
}
