using BeatLeader.Core.Managers.ReplayEnhancer;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace BeatLeader
{
    [HarmonyPatch]
    class LevelDataEnhancerPatch {
        static MethodInfo TargetMethod() {
            return AccessTools.GetDeclaredMethods(typeof(StandardLevelScenesTransitionSetupDataSO))
            .FirstOrDefault(m => m.Name == "Init" &&
                                 m.GetParameters().Length == 20);
        }

        public static void Postfix(
        string gameMode,
        ref BeatmapKey beatmapKey,
        BeatmapLevel beatmapLevel,
        OverrideEnvironmentSettings? overrideEnvironmentSettings,
        ColorScheme? playerOverrideColorScheme,
        bool playerOverrideLightshowColors,
        GameplayModifiers gameplayModifiers,
        PlayerSpecificSettings playerSpecificSettings,
        PracticeSettings? practiceSettings,
        EnvironmentsListModel environmentsListModel) {
            Plugin.Log.Debug($"LevelDataEnhancerPatch.postfix {beatmapKey.levelId} {beatmapLevel.songName}");

            string environmentName = beatmapLevel.GetEnvironmentName(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);

            if (overrideEnvironmentSettings?.overrideEnvironments == true) {
                environmentName = overrideEnvironmentSettings
                    .GetOverrideEnvironmentInfoForType(
                        environmentsListModel
                            .GetEnvironmentInfoBySerializedName(environmentName)
                            .environmentType)
                    .environmentName;
            }

            MapEnhancer.beatmapKey = beatmapKey;
            MapEnhancer.beatmapLevel = beatmapLevel;
            MapEnhancer.gameplayModifiers = gameplayModifiers;
            MapEnhancer.playerSpecificSettings = playerSpecificSettings;
            MapEnhancer.practiceSettings = practiceSettings;
            MapEnhancer.environmentName = environmentName;
            MapEnhancer.colorScheme = playerOverrideColorScheme;
        }
    }

    [HarmonyPatch]
    class LevelDataEnhancerPatch2 {

        static MethodInfo TargetMethod() {
            return AccessTools.GetDeclaredMethods(typeof(StandardLevelScenesTransitionSetupDataSO))
                .FirstOrDefault(m => m.Name == "Init" &&
                                     m.GetParameters().Length == 20);
        }

        static void Postfix(
            string gameMode,
            ref BeatmapKey beatmapKey,
            BeatmapLevel beatmapLevel,
            OverrideEnvironmentSettings? overrideEnvironmentSettings,
            ColorScheme? playerOverrideColorScheme,
            bool playerOverrideLightshowColors,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings,
            PracticeSettings? practiceSettings,
            EnvironmentsListModel environmentsListModel) {
            LevelDataEnhancerPatch.Postfix(
                gameMode,
                ref beatmapKey,
                beatmapLevel,
                overrideEnvironmentSettings,
                playerOverrideColorScheme,
                playerOverrideLightshowColors,
                gameplayModifiers,
                playerSpecificSettings,
                practiceSettings,
                environmentsListModel);
        }
    }
}
