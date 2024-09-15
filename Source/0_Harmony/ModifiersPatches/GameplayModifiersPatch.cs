using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch(typeof(GameplayModifiers), "get_songSpeedMul")]
    internal static class GameplayModifiersPatch {
        
        [UsedImplicitly]
        internal static void Postfix(ref float __result) {
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
