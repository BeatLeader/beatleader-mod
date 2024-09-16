using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    [HarmonyPatch(typeof(GameplayCoreInstaller), "InstallBindings")]
    internal static class GameplayCoreInstallerPatch {
        internal static GameplayCoreInstaller? instance;

        [UsedImplicitly]
        private static void Prefix(GameplayCoreInstaller __instance) {
            instance = __instance;
        }
    }

    [HarmonyPatch(typeof(GameplayModifiers), "get_songSpeedMul")]
    internal static class GameplayModifiersPatch {
        internal static void Postfix(GameplayModifiers __instance, ref float __result) {
            if (__instance != GameplayCoreInstallerPatch.instance?._sceneSetupData.gameplayModifiers) return;

            var speed = SpeedModifiers.SongSpeed();
            if (speed == 1f) return;
            __result = speed;
        }
    }

    [HarmonyPatch(typeof(BeatmapObjectSpawnMovementData), "Init")]
    [HarmonyBefore(new string[] { "com.zephyr.BeatSaber.JDFixer" })]
    internal class NJSPatch
    {
        [UsedImplicitly]
        internal static void Prefix(ref float startNoteJumpMovementSpeed)
        {
            if (BS_Utils.Plugin.LevelData != null && BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData != null)
            {
                var speed = SpeedModifiers.SongSpeed();
                if (speed == 1f) return;
                // We want to compensate the speed of the AudioTimeSyncController by doing the opposite with the NJS.
                startNoteJumpMovementSpeed = ((startNoteJumpMovementSpeed * speed - startNoteJumpMovementSpeed) / 2 + startNoteJumpMovementSpeed) / speed;
            }
        }
    }
}
