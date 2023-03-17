using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Utils {
    public static class ReplayDataHelper {

        #region Scenes Management

        public static readonly EnvironmentTypeSO NormalEnvironmentType = Resources.FindObjectsOfTypeAll<EnvironmentTypeSO>()
                .FirstOrDefault(x => x.typeNameLocalizationKey == "NORMAL_ENVIRONMENT_TYPE");

        public static readonly StandardLevelScenesTransitionSetupDataSO StandardLevelScenesTransitionSetupDataSO = Resources
                .FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();

        public static StandardLevelScenesTransitionSetupDataSO CreateTransitionData(
            this ReplayLaunchData launchData, PlayerDataModel playerModel) {
            var transitionData = StandardLevelScenesTransitionSetupDataSO;
            var playerData = playerModel.playerData;

            var overrideEnv = launchData.EnvironmentInfo != null;
            var envSettings = overrideEnv ? new() : playerData.overrideEnvironmentSettings;
            if (overrideEnv) {
                envSettings.overrideEnvironments = true;
                envSettings.SetEnvironmentInfoForType(
                    NormalEnvironmentType, launchData.EnvironmentInfo);
            }

            var replay = launchData.MainReplay;
            var practiceSettings = launchData.IsBattleRoyale 
                ? null : replay.GetPracticeSettingsFromReplay();
            var beatmap = launchData.DifficultyBeatmap;

            transitionData?.Init("Solo", beatmap, beatmap!.level, envSettings,
                playerData.colorSchemesSettings.GetOverrideColorScheme(),
                replay.GetModifiersFromReplay(),
                playerData.playerSpecificSettings.GetPlayerSettingsByReplay(replay),
                practiceSettings, "Menu");

            return transitionData!;
        }

        public static EnvironmentInfoSO GetEnvironmentByName(string name) {
            return Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>()
                .FirstOrDefault(x => x.environmentName == name);
        }

        #endregion

        #region Data Management

        public static string ParseModifierLocalizationKeyToServerName(this string modifierLocalizationKey) {
            if (string.IsNullOrEmpty(modifierLocalizationKey)) return modifierLocalizationKey;

            int idx1 = modifierLocalizationKey.IndexOf('_') + 1;
            var char1 = modifierLocalizationKey[idx1];

            int idx2 = modifierLocalizationKey.IndexOf('_', idx1) + 1;
            var char2 = modifierLocalizationKey[idx2];

            return $"{char.ToUpper(char1)}{char.ToUpper(char2)}";
        }

        private static GameplayModifiers.SongSpeed SongSpeedFromModifiers(string modifiers) {
            if (modifiers.Contains("SS")) return GameplayModifiers.SongSpeed.Slower;
            if (modifiers.Contains("SF")) return GameplayModifiers.SongSpeed.SuperFast;
            if (modifiers.Contains("FS")) return GameplayModifiers.SongSpeed.Faster;

            return GameplayModifiers.SongSpeed.Normal;
        }

        public static GameplayModifiers GetModifiersFromReplay(this Replay replay) {
            var modifiers = replay.info.modifiers;

            return new GameplayModifiers(
                energyType: modifiers.Contains("BE") ? GameplayModifiers.EnergyType.Battery : GameplayModifiers.EnergyType.Bar,
                noFailOn0Energy: modifiers.Contains("NF"),
                instaFail: modifiers.Contains("IF"),
                failOnSaberClash: modifiers.Contains("CS"),
                enabledObstacleType: modifiers.Contains("NO") ? GameplayModifiers.EnabledObstacleType.NoObstacles : GameplayModifiers.EnabledObstacleType.All,
                noBombs: modifiers.Contains("NB"),
                fastNotes: false,
                strictAngles: modifiers.Contains("SA"),
                disappearingArrows: modifiers.Contains("DA"),
                songSpeed: SongSpeedFromModifiers(modifiers),
                noArrows: modifiers.Contains("NA"),
                ghostNotes: modifiers.Contains("GN"),
                proMode: modifiers.Contains("PM"),
                zenMode: false,
                smallCubes: modifiers.Contains("SC"));
        }
        public static PracticeSettings? GetPracticeSettingsFromReplay(this Replay replay) {
            return replay.info.speed == 0 ? null : new(replay.info.startTime, replay.info.speed);
        }
        public static PlayerSpecificSettings GetPlayerSettingsByReplay(this PlayerSpecificSettings settings, Replay replay) {
            return settings.CopyWith(replay.info.leftHanded, automaticPlayerHeight: true);
        }
        public static bool TryGetFrameByTime(this LinkedListNode<Frame> entryPoint, float time, out LinkedListNode<Frame>? frame) {
            for (frame = entryPoint; frame != null; frame = frame.Next) {
                if (frame.Value.time >= time) return true;
            }
            frame = null;
            return false;
        }

        #endregion

        #region Computing

        public static int ComputeObstacleId(this ObstacleData obstacleData) {
            return obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
        }

        public static int ComputeNoteId(this NoteData noteData, bool noscoring = false, bool altBomb = false) {
            // Bombs may have both correct values as well as default.
            int colorType = altBomb && noteData.colorType == ColorType.None ? 0 : (int)noteData.colorType;
            int cutDirection = altBomb && noteData.colorType == ColorType.None ? 3 : (int)noteData.cutDirection;

            // Pre 1.20 replays has no scoring in ID
            int scoringPart = noscoring ? 0 : ((int)noteData.scoringType + 2) * 10000;

            return scoringPart + noteData.lineIndex
                * 1000 + (int)noteData.noteLineLayer * 100 + colorType
                * 10 + cutDirection;
        }

        public static bool IsMatch(this NoteEvent noteEvent, NoteData noteData) {
            var id = noteEvent.noteID;
            return id == noteData.ComputeNoteId() 
                || id == noteData.ComputeNoteId(true, false)
                || id == noteData.ComputeNoteId(true, true)
                || id == noteData.ComputeNoteId(false, true);
        }

        #endregion
    }
}