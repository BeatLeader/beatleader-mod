using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Installers;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using System;

namespace BeatLeader
{
    [HarmonyPatch(
        typeof(StandardLevelScenesTransitionSetupDataSO),
        nameof(StandardLevelScenesTransitionSetupDataSO.Init),
        new Type[] { typeof(string), typeof(IBeatmapLevelData), typeof(BeatmapKey), typeof(BeatmapLevel), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(EnvironmentsListModel), typeof(AudioClipAsyncLoader), typeof(BeatmapDataLoader), typeof(string), typeof(bool), typeof(bool), typeof(RecordingToolManager.SetupData?) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal }
        )]
    class LevelDataEnhancerPatch
    {
        public static void Postfix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, EnvironmentsListModel environmentsListModel)
        {
            Plugin.Log.Debug($"LevelDataEnhancerPatch.postfix {beatmapKey.levelId} {beatmapLevel.songName}");
            string environmentName = beatmapLevel.GetEnvironmentName(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);
            EnvironmentInfoSO? environmentInfoSO = environmentsListModel.GetEnvironmentInfoBySerializedName(environmentName);
            if (environmentInfoSO != null && overrideEnvironmentSettings is { overrideEnvironments: true })
            {
                environmentName = overrideEnvironmentSettings.GetOverrideEnvironmentInfoForType(environmentInfoSO.environmentType).environmentName;
            }
            MapEnhancer.beatmapKey = beatmapKey;
            MapEnhancer.beatmapLevel = beatmapLevel;
            MapEnhancer.gameplayModifiers = gameplayModifiers;
            MapEnhancer.playerSpecificSettings = playerSpecificSettings;
            MapEnhancer.practiceSettings = practiceSettings;
            MapEnhancer.environmentName = environmentName;
            MapEnhancer.colorScheme = overrideColorScheme;
        }
    }

    [HarmonyPatch(
        typeof(StandardLevelScenesTransitionSetupDataSO),
        nameof(StandardLevelScenesTransitionSetupDataSO.Init),
        new Type[] { typeof(string), typeof(BeatmapKey), typeof(BeatmapLevel), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(EnvironmentsListModel), typeof(AudioClipAsyncLoader), typeof(BeatmapDataLoader), typeof(string), typeof(BeatmapLevelsModel), typeof(bool), typeof(bool), typeof(RecordingToolManager.SetupData?) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal }
        )]
    class LevelDataEnhancerPatch2
    {
        static void Postfix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, EnvironmentsListModel environmentsListModel)
        {
            LevelDataEnhancerPatch.Postfix(beatmapKey, beatmapLevel, overrideEnvironmentSettings, overrideColorScheme, gameplayModifiers, playerSpecificSettings, practiceSettings, environmentsListModel);
        }
    }
}
