using BeatLeader.Core.Managers.ReplayEnhancer;
using HarmonyLib;

namespace BeatLeader {
    [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
    class MultiplayerLevelDataEnhancerPatch
    {
        static void Postfix(MultiplayerLevelScenesTransitionSetupDataSO __instance, IPreviewBeatmapLevel previewBeatmapLevel, IDifficultyBeatmap difficultyBeatmap, ColorScheme overrideColorScheme,
            GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, bool useTestNoteCutSoundEffects = false)
        {
            Plugin.Log.Debug("MultiplayerLevelDataEnhancerPatch.postfix");
            EnvironmentInfoSO environmentInfoSO = difficultyBeatmap.GetEnvironmentInfo();
            MapEnhancer.difficultyBeatmap = difficultyBeatmap;
            MapEnhancer.previewBeatmapLevel = previewBeatmapLevel;
            MapEnhancer.gameplayModifiers = gameplayModifiers;
            MapEnhancer.playerSpecificSettings = playerSpecificSettings;
            MapEnhancer.practiceSettings = practiceSettings;
            MapEnhancer.useTestNoteCutSoundEffects = useTestNoteCutSoundEffects;
            MapEnhancer.environmentInfo = environmentInfoSO;
            MapEnhancer.colorScheme = overrideColorScheme;
        }
    }
}
