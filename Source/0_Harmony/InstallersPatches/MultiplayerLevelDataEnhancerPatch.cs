using BeatLeader.Core.Managers.ReplayEnhancer;
using HarmonyLib;

namespace BeatLeader {
    [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
    class MultiplayerLevelDataEnhancerPatch
    {
        static void Postfix(MultiplayerLevelScenesTransitionSetupDataSO __instance, string gameMode, in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, IBeatmapLevelData beatmapLevelData, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, AudioClipAsyncLoader audioClipAsyncLoader, BeatmapDataLoader beatmapDataLoader, bool useTestNoteCutSoundEffects = false)
        {
            Plugin.Log.Debug("MultiplayerLevelDataEnhancerPatch.postfix");
            MapEnhancer.beatmapKey = beatmapKey;
            MapEnhancer.beatmapLevel = beatmapLevel;
            MapEnhancer.gameplayModifiers = gameplayModifiers;
            MapEnhancer.playerSpecificSettings = playerSpecificSettings;
            MapEnhancer.practiceSettings = practiceSettings;
            MapEnhancer.environmentName = beatmapLevel.GetEnvironmentName(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);
            MapEnhancer.colorScheme = overrideColorScheme;
        }
    }
}
