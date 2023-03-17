using BeatLeader.Manager;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader.UIPatches {
    [HarmonyPatch(typeof(GameScenesManager), "ScenesTransitionCoroutine")]
    internal static class HideModalsOnPlay {
        [UsedImplicitly]
        private static void Prefix() {
            LeaderboardEvents.FireHideAllOtherModalsEvent(null);
        }
    }
}