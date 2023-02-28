using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Utils {
    public static class ReplayDataHelper {
        private class EditableModifiers : GameplayModifiers {
            public new EnergyType energyType {
                get => this._energyType;
                set => this._energyType = value;
            }

            public new bool noFailOn0Energy {
                get => this._noFailOn0Energy;
                set => this._noFailOn0Energy = value;
            }

            public new bool instaFail {
                get => this._instaFail;
                set => this._instaFail = value;
            }

            public new bool failOnSaberClash {
                get => this._failOnSaberClash;
                set => this._failOnSaberClash = value;
            }

            public new EnabledObstacleType enabledObstacleType {
                get => this._enabledObstacleType;
                set => this._enabledObstacleType = value;
            }

            public new bool fastNotes {
                get => this._fastNotes;
                set => this._fastNotes = value;
            }

            public new bool strictAngles {
                get => this._strictAngles;
                set => this._strictAngles = value;
            }

            public new bool disappearingArrows {
                get => this._disappearingArrows;
                set => this._disappearingArrows = value;
            }

            public new bool ghostNotes {
                get => this._ghostNotes;
                set => this._ghostNotes = value;
            }

            public new bool noBombs {
                get => this._noBombs;
                set => this._noBombs = value;
            }

            public new SongSpeed songSpeed {
                get => this._songSpeed;
                set => this._songSpeed = value;
            }

            public new bool noArrows {
                get => this._noArrows;
                set => this._noArrows = value;
            }

            public new bool proMode {
                get => this._proMode;
                set => this._proMode = value;
            }

            public new bool zenMode {
                get => this._zenMode;
                set => this._zenMode = value;
            }

            public new bool smallCubes {
                get => this._smallCubes;
                set => this._smallCubes = value;
            }
        }

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
        public static GameplayModifiers GetModifiersFromReplay(this Replay replay) {
            EditableModifiers replayModifiers = new();
            string[] modifiers = replay.info.modifiers.Split(',');
            foreach (string modifier in modifiers) {
                switch (modifier) {
                    case "DA":
                        replayModifiers.disappearingArrows = true;
                        break;
                    case "FS":
                        replayModifiers.songSpeed = GameplayModifiers.SongSpeed.Faster;
                        break;
                    case "SS":
                        replayModifiers.songSpeed = GameplayModifiers.SongSpeed.Slower;
                        break;
                    case "SF":
                        replayModifiers.songSpeed = GameplayModifiers.SongSpeed.SuperFast;
                        break;
                    case "GN":
                        replayModifiers.ghostNotes = true;
                        break;
                    case "NA":
                        replayModifiers.noArrows = true;
                        break;
                    case "NB":
                        replayModifiers.noBombs = true;
                        break;
                    case "NF":
                        replayModifiers.noFailOn0Energy = true;
                        break;
                    case "NO":
                        replayModifiers.enabledObstacleType = GameplayModifiers.EnabledObstacleType.NoObstacles;
                        break;
                    case "SA":
                        replayModifiers.strictAngles = true;
                        break;
                    case "PM":
                        replayModifiers.proMode = true;
                        break;
                    case "SC":
                        replayModifiers.smallCubes = true;
                        break;
                    case "CS":
                        replayModifiers.failOnSaberClash = true;
                        break;
                    case "IF":
                        replayModifiers.instaFail = true;
                        break;
                    case "BE":
                        replayModifiers.energyType = GameplayModifiers.EnergyType.Battery;
                        break;
                }
            }

            return replayModifiers;
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

        public static int ComputeNoteId(this NoteData noteData) {
            return ((int)noteData.scoringType + 2) * 10000 + noteData.lineIndex
                * 1000 + (int)noteData.noteLineLayer * 100 + (int)noteData.colorType
                * 10 + (int)noteData.cutDirection;
        }

        public static bool IsMatch(this NoteEvent noteEvent, NoteData noteData) {
            var calcId = noteData.ComputeNoteId();
            var id = noteEvent.noteID;
            return id == calcId || id == calcId - 30000;
        }

        #endregion
    }
}