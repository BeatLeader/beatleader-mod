using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Installers;
using BeatSaber.PerformancePresets;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Reflection;

namespace BeatLeader
{
    [HarmonyPatch]
    class LevelDataEnhancerPatch {
        static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(StandardLevelScenesTransitionSetupDataSO),
            m => m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init) &&
                 m.GetParameters().All(p => p.ParameterType != typeof(IBeatmapLevelData)));
        public static void Postfix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, EnvironmentsListModel environmentsListModel) {
            Plugin.Log.Debug($"LevelDataEnhancerPatch.postfix {beatmapKey.levelId} {beatmapLevel.songName}");
            string environmentName = beatmapLevel.GetEnvironmentName(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);
            EnvironmentInfoSO? environmentInfoSO = environmentsListModel.GetEnvironmentInfoBySerializedName(environmentName);
            if (environmentInfoSO != null && overrideEnvironmentSettings is { overrideEnvironments: true }) {
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

    [HarmonyPatch]
    class LevelDataEnhancerPatch2 {

        static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(StandardLevelScenesTransitionSetupDataSO),
            m => m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init) &&
                 m.GetParameters().Any(p => p.ParameterType == typeof(IBeatmapLevelData)));
        static void Postfix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, EnvironmentsListModel environmentsListModel) {
            LevelDataEnhancerPatch.Postfix(beatmapKey, beatmapLevel, overrideEnvironmentSettings, overrideColorScheme, gameplayModifiers, playerSpecificSettings, practiceSettings, environmentsListModel);
        }
    }
}
