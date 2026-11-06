using BeatLeader.Core.Managers.ReplayEnhancer;
using HarmonyLib;

namespace BeatLeader {
    [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupData), nameof(MultiplayerLevelScenesTransitionSetupData.Init))]
    class MultiplayerLevelDataEnhancerPatch
    {
        static void Postfix(MultiplayerLevelScenesTransitionSetupData __instance, string gameMode, in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, IBeatmapLevelData beatmapLevelData, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, AudioClipAsyncLoader audioClipAsyncLoader, BeatmapDataLoader beatmapDataLoader)
        {
            Plugin.Log.Debug("MultiplayerLevelDataEnhancerPatch.postfix");
            MapEnhancer.beatmapKey = beatmapKey;
            MapEnhancer.beatmapLevel = beatmapLevel;
            MapEnhancer.gameplayModifiers = gameplayModifiers;
            MapEnhancer.playerSpecificSettings = playerSpecificSettings;
            MapEnhancer.environmentName = beatmapLevel.GetEnvironmentName(beatmapKey.characteristic, beatmapKey.difficulty);
            MapEnhancer.colorScheme = overrideColorScheme;
        }
    }
}
