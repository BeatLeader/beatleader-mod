using BeatLeader.Replayer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
