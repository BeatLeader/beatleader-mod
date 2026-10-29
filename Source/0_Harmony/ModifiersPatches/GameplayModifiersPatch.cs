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

    // Since 1.40 the starting NJS goes to VariableMovementDataProvider instead of BeatmapObjectSpawnMovementData
    [HarmonyPatch(typeof(VariableMovementDataProvider), nameof(VariableMovementDataProvider.Init))]
    [HarmonyBefore(new string[] { "com.zephyr.BeatSaber.JDFixer" })]
    internal class NJSPatch
    {
        [UsedImplicitly]
        internal static void Prefix(ref float noteJumpMovementSpeed, ref float minRelativeNoteJumpSpeed)
        {
            var multiplier = NoteJumpSpeedMultiplier();
            if (multiplier == 1f) return;
            noteJumpMovementSpeed *= multiplier;
            // NJS events are offsets from the starting NJS, so they have to be scaled the same way
            // or the jump durations and spawn-ahead time get out of proportion
            minRelativeNoteJumpSpeed *= multiplier;
        }

        // We want to compensate the speed of the AudioTimeSyncController by doing the opposite with the NJS,
        // so notes get only half of the speed-up: njs * (1 + (speed - 1) / 2) / speed
        internal static float NoteJumpSpeedMultiplier()
        {
            var setupData = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
            // Practice speed replaces the modifier one in GameplayCoreInstaller, so there is nothing to compensate
            if (setupData == null || setupData.practiceSettings != null) return 1f;

            var speed = SpeedModifiers.SongSpeed();
            return (1 + (speed - 1) / 2) / speed;
        }
    }

    [HarmonyPatch(typeof(VariableMovementDataProvider), "HandleNoteJumpMovementSpeedEvent")]
    internal class NJSEventPatch
    {
        [UsedImplicitly]
        internal static void Postfix(VariableMovementDataProvider __instance)
        {
            var multiplier = NJSPatch.NoteJumpSpeedMultiplier();
            if (multiplier == 1f) return;
            ref var interpolation = ref __instance._relativeNoteJumpSpeedInterpolation;
            interpolation._fromValue *= multiplier;
            interpolation._toValue *= multiplier;
        }
    }
}
