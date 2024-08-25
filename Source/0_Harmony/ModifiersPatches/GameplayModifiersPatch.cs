using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    [HarmonyPatch(typeof(GameplayCoreInstaller), "InstallBindings")]
    internal static class GameplayCoreInstallerPatch {
        [UsedImplicitly]
        private static void Prefix(GameplayCoreInstaller __instance) {
            var speed = SpeedModifiers.GetSongSpeed();
            var modifiers = __instance._sceneSetupData.gameplayModifiers.CopyWith(songSpeed: speed);
            __instance._sceneSetupData.SetField("gameplayModifiers", modifiers);
        }
    }

    [HarmonyPatch(typeof(BeatmapObjectSpawnMovementData), "Init")]
    [HarmonyBefore(new string[] { "com.zephyr.BeatSaber.JDFixer" })]
    internal class NJSPatch {
        [UsedImplicitly]
        internal static void Prefix(ref float startNoteJumpMovementSpeed) {
            if (BS_Utils.Plugin.LevelData != null && BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData != null) {
                var speed = SpeedModifiers.GetSongSpeedMultiplier();
                if (Mathf.Approximately(speed, 1f)) return;
                // We want to compensate the speed of the AudioTimeSyncController by doing the opposite with the NJS.
                startNoteJumpMovementSpeed = ((startNoteJumpMovementSpeed * speed - startNoteJumpMovementSpeed) / 2 + startNoteJumpMovementSpeed) / speed;
            }
        }
    }
}