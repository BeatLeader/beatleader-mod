using BeatLeader.Manager;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader.UIPatches {
    [HarmonyPatch(typeof(GameScenesManager), "ScenesTransitionCoroutine")]
    internal static class HideScoreModalOnPlay {
        [UsedImplicitly]
        private static void Prefix() {
            LeaderboardEvents.NotifySceneTransitionStarted();
        }
    }
}