using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;

namespace BeatLeader.Utils {
    internal static class AbstractReplayUtils {
        private static readonly EnvironmentType normalEnvironmentType = EnvironmentType.Normal;

        private static readonly StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSo =
            Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault()!;

        public static StandardLevelScenesTransitionSetupDataSO CreateTransitionData(this ReplayLaunchData launchData, PlayerDataModel playerModel) {
            var transitionData = standardLevelScenesTransitionSetupDataSo;
            var playerData = playerModel.playerData;

            var overrideEnv = launchData.EnvironmentInfo != null;
            var envSettings = playerData.overrideEnvironmentSettings;
            if (overrideEnv) {
                envSettings = new() { overrideEnvironments = true };
                envSettings.SetEnvironmentInfoForType(
                    normalEnvironmentType, launchData.EnvironmentInfo
                );
            }

            var replay = launchData.MainReplay;
            var practiceSettings = launchData.IsBattleRoyale ? null
                : launchData.MainReplay.ReplayData.PracticeSettings;

            transitionData.Init(
                "Solo",
                (BeatmapKey)launchData.BeatmapKey,
                launchData.BeatmapLevel,
                envSettings,
                playerData.colorSchemesSettings.GetOverrideColorScheme(),
                null,
                replay.ReplayData.GameplayModifiers,
                playerData.playerSpecificSettings.GetPlayerSettingsByReplay(replay),
                practiceSettings,
                null,
                null,
                null,
                "Menu"
            );

            return transitionData;
        }

        public static NoteCutInfo SaturateNoteCutInfo(this NoteCutInfo cutInfo, NoteData data) {
            return new NoteCutInfo(
                data,
                cutInfo.speedOK,
                cutInfo.directionOK,
                cutInfo.saberTypeOK,
                cutInfo.wasCutTooSoon,
                cutInfo.saberSpeed,
                cutInfo.saberDir,
                cutInfo.saberType,
                cutInfo.timeDeviation,
                cutInfo.cutDirDeviation,
                cutInfo.cutPoint,
                cutInfo.cutNormal,
                cutInfo.cutDistanceToCenter,
                cutInfo.cutAngle,
                cutInfo.worldRotation,
                cutInfo.inverseWorldRotation,
                cutInfo.noteRotation,
                cutInfo.notePosition,
                cutInfo.saberMovementData
            );
        }

        private static PlayerSpecificSettings GetPlayerSettingsByReplay(this PlayerSpecificSettings settings, IReplay replay) {
            return settings.CopyWith(replay.ReplayData.LeftHanded, replay.ReplayData.FixedHeight, replay.ReplayData.FixedHeight is null);
        }
    }
}