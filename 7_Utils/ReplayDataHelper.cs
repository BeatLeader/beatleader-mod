using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class ReplayDataHelper
    {
        private class EditableModifiers : GameplayModifiers
        {
            public new EnergyType energyType
            {
                get => this._energyType;
                set => this._energyType = value;
            }

            public new bool noFailOn0Energy
            {
                get => this._noFailOn0Energy;
                set => this._noFailOn0Energy = value;
            }

            public new bool instaFail
            {
                get => this._instaFail;
                set => this._instaFail = value;
            }

            public new bool failOnSaberClash
            {
                get => this._failOnSaberClash;
                set => this._failOnSaberClash = value;
            }

            public new EnabledObstacleType enabledObstacleType
            {
                get => this._enabledObstacleType;
                set => this._enabledObstacleType = value;
            }

            public new bool fastNotes
            {
                get => this._fastNotes;
                set => this._fastNotes = value;
            }

            public new bool strictAngles
            {
                get => this._strictAngles;
                set => this._strictAngles = value;
            }

            public new bool disappearingArrows
            {
                get => this._disappearingArrows;
                set => this._disappearingArrows = value;
            }

            public new bool ghostNotes
            {
                get => this._ghostNotes;
                set => this._ghostNotes = value;
            }

            public new bool noBombs
            {
                get => this._noBombs;
                set => this._noBombs = value;
            }

            public new SongSpeed songSpeed
            {
                get => this._songSpeed;
                set => this._songSpeed = value;
            }

            public new bool noArrows
            {
                get => this._noArrows;
                set => this._noArrows = value;
            }

            public new bool proMode
            {
                get => this._proMode;
                set => this._proMode = value;
            }

            public new bool zenMode
            {
                get => this._zenMode;
                set => this._zenMode = value;
            }

            public new bool smallCubes
            {
                get => this._smallCubes;
                set => this._smallCubes = value;
            }
        }

        #region Scenes Management

        public static readonly EnvironmentTypeSO NormalEnvironmentType = Resources.FindObjectsOfTypeAll<EnvironmentTypeSO>()
                .FirstOrDefault(x => x.typeNameLocalizationKey == "NORMAL_ENVIRONMENT_TYPE");

        public static readonly StandardLevelScenesTransitionSetupDataSO StandardLevelScenesTransitionSetupDataSO = Resources
                .FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();

        public static StandardLevelScenesTransitionSetupDataSO CreateTransitionData(this Replay replay,
            PlayerDataModel playerModel, IDifficultyBeatmap difficulty, EnvironmentInfoSO environment = null)
        {
            var data = StandardLevelScenesTransitionSetupDataSO;
            var playerData = playerModel.playerData;
            bool overrideEnv = environment != null;

            var environmentSettings = overrideEnv ? new OverrideEnvironmentSettings() : playerData.overrideEnvironmentSettings;
            if (overrideEnv)
            {
                environmentSettings.overrideEnvironments = true;
                environmentSettings.SetEnvironmentInfoForType(NormalEnvironmentType, environment);
            }

            data?.Init("Solo", difficulty, difficulty.level, environmentSettings,
                playerData.colorSchemesSettings.GetOverrideColorScheme(),
                replay.GetModifiersFromReplay(),
                playerData.playerSpecificSettings.GetPlayerSettingsByReplay(replay),
                replay.CreatePracticeSettingsFromReplay(), "Menu");

            return data;
        }
        public static string GetEnvironmentSerializedNameByEnvironmentName(string name)
        {
            name = name.Replace(" ", "");
            if (name != "TheFirst" && name != "Spooky" && name != "FallOutBoy")
            {
                return name + "Environment";
            }
            else
            {
                switch (name)
                {
                    case "TheFirst":
                        return "DefaultEnvironment";
                    case "Spooky":
                        return "HalloweenEnvironment";
                    case "FallOutBoy":
                        return "PyroEnvironment";
                }
            }

            return "!UNDEFINED!";
        }
        public static EnvironmentInfoSO GetEnvironmentBySerializedName(string name)
        {
            return Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>().First(x => x.serializedName == name);
        }
        public static EnvironmentInfoSO GetEnvironmentByName(string name)
        {
            return Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>().FirstOrDefault(x =>
                x.serializedName == GetEnvironmentSerializedNameByEnvironmentName(name));
        }

        #endregion

        #region Data Management

        public static string ParseModifierLocalizationKeyToServerName(this string modifierLocalizationKey)
        {
            if (string.IsNullOrEmpty(modifierLocalizationKey)) return modifierLocalizationKey;

            int idx1 = modifierLocalizationKey.IndexOf('_') + 1;
            var char1 = modifierLocalizationKey[idx1];

            int idx2 = modifierLocalizationKey.IndexOf('_', idx1) + 1;
            var char2 = modifierLocalizationKey[idx2];

            return $"{char.ToUpper(char1)}{char.ToUpper(char2)}";
        }
        public static GameplayModifiers GetModifiersFromReplay(this Replay replay)
        {
            EditableModifiers replayModifiers = new EditableModifiers();
            string[] modifiers = replay.info.modifiers.Split(',');
            foreach (string modifier in modifiers)
            {
                switch (modifier)
                {
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
        public static PracticeSettings CreatePracticeSettingsFromReplay(this Replay replay)
        {
            return new PracticeSettings(replay.info.startTime, replay.info.speed);
        }
        public static PracticeSettings GetPracticeSettingsFromReplay(this Replay replay)
        {
            return replay.info.startTime != 0 ? CreatePracticeSettingsFromReplay(replay) : null;
        }
        public static PlayerSpecificSettings GetPlayerSettingsByReplay(this PlayerSpecificSettings settings, Replay replay)
        {
            return settings.CopyWith(replay.info.leftHanded, replay.info.height, false);
        }
        public static NoteEvent GetNoteEvent(this NoteData noteData, Replay replay)
        {
            return GetNoteEventById(replay, noteData.ComputeNoteId(), noteData.time);
        }
        public static NoteEvent GetNoteEvent(this NoteController noteController, Replay replay)
        {
            return GetNoteEventInOrder(noteController, replay).Item2;
        }
        public static (int, NoteEvent) GetNoteEventInOrder(this NoteController noteController, Replay replay)
        {
            return GetNoteEventByIdInOrder(replay, noteController.noteData.ComputeNoteId(), noteController.noteData.time);
        }
        public static WallEvent GetWallEvent(this ObstacleData obstacleData, Replay replay)
        {
            return GetWallEventById(replay, obstacleData.ComputeObstacleId(), obstacleData.time);
        }
        public static NoteEvent GetNoteEventById(this Replay replay, int ID, float time)
        {
            return GetNoteEventByIdInOrder(replay, ID, time).Item2;
        }
        public static (int, NoteEvent) GetNoteEventByIdInOrder(this Replay replay, int ID, float time)
        {
            for (int i = 0; i < replay.notes.Count; i++)
            {
                var item = replay.notes[i];
                if ((item.noteID == ID || item.noteID == ID - 30000) && item.spawnTime >= time)
                    return (i, item);
            }
            return default;
        }
        public static WallEvent GetWallEventById(this Replay replay, int ID, float time)
        {
            foreach (var item in replay.walls)
                if (item.wallID == ID && (item.spawnTime >= time - 0.05f && item.spawnTime <= time + 0.15f))
                    return item;
            return null;
        }
        public static NoteCutInfo GetNoteCutInfo(this NoteController noteController, NoteEvent noteEvent)
        {
            return ReplayNoteCutInfo.Convert(noteEvent.noteCutInfo, noteController);
        }
        public static NoteCutInfo GetNoteCutInfo(this NoteController noteController, Replay replay)
        {
            var noteEvent = GetNoteEvent(noteController, replay);
            return noteEvent != null ? ReplayNoteCutInfo.Convert(noteEvent.noteCutInfo, noteController) : default;
        }
        public static LinkedListNode<Frame> GetFrameByTime(this LinkedList<Frame> frames, float time)
        {
            TryGetFrameByTime(frames, time, out var frame);
            return frame;
        }
        public static bool TryGetFrameByTime(this LinkedListNode<Frame> entryPoint, float time, out LinkedListNode<Frame> frame)
        {
            for (frame = entryPoint; frame != null; frame = frame.Next)
            {
                if (frame.Value.time >= time)
                {
                    return true;
                }
            }

            frame = null;
            return false;
        }
        public static bool TryGetFrameByTime(this LinkedList<Frame> frames, float time, out LinkedListNode<Frame> frame)
        {
            return TryGetFrameByTime(frames.First, time, out frame);
        }

        #endregion

        #region Computing

        public static void DecodeNoteId(int noteId,
            out NoteData.ScoringType scoringType,
            out int lineIndex,
            out NoteLineLayer noteLineLayer,
            out ColorType colorType,
            out NoteCutDirection cutDirection
        ) {
            cutDirection = (NoteCutDirection)(noteId % 10);
            noteId /= 10;
            colorType = (ColorType)(noteId % 10);
            noteId /= 10;
            noteLineLayer = (NoteLineLayer)(noteId % 10);
            noteId /= 10;
            lineIndex = noteId % 10;
            noteId /= 10;
            scoringType = (NoteData.ScoringType)((noteId -= 2) < -1 ? noteId + 3 : noteId);
        }
        public static int ComputeObstacleId(this ObstacleData obstacleData)
        {
            return obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
        }
        public static int ComputeNoteId(this NoteData noteData)
        {
            return ((int)noteData.scoringType + 2) * 10000 + noteData.lineIndex
                * 1000 + (int)noteData.noteLineLayer * 100 + (int)noteData.colorType
                * 10 + (int)noteData.cutDirection;
        }
        public static int ComputeNoteScore(this NoteEvent note)
        {
            if (note.eventType != NoteEventType.good)
            {
                return note.eventType switch
                {
                    NoteEventType.bad => -2,
                    NoteEventType.miss => -3,
                    NoteEventType.bomb => -4,
                    _ => -1
                };
            }

            var noteCutInfo = note.noteCutInfo;

            double beforeCutRawScore = Mathf.Clamp((float)Math.Round(70 * noteCutInfo.beforeCutRating), 0, 70);
            double afterCutRawScore = Mathf.Clamp((float)Math.Round(30 * noteCutInfo.afterCutRating), 0, 30);

            double num = 1 - Mathf.Clamp(noteCutInfo.cutDistanceToCenter / 0.3f, 0.0f, 1.0f);
            double cutDistanceRawScore = Math.Round(15 * num);

            return (int)beforeCutRawScore + (int)afterCutRawScore + (int)cutDistanceRawScore;
        }
        public static float ComputeEnergyChange(this NoteData.GameplayType type, bool allIsOK, bool miss)
        {
            switch (type)
            {
                case NoteData.GameplayType.Normal:
                case NoteData.GameplayType.BurstSliderHead:
                    return miss ? (-0.15f) : allIsOK ? 0.01f : (-0.1f);
                case NoteData.GameplayType.Bomb:
                    return miss ? 0 : (-0.15f);
                case NoteData.GameplayType.BurstSliderElement:
                    return miss ? (-0.03f) : allIsOK ? 0.002f : (-0.025f);
                default:
                    return 0;
            }
        }
        public static ScoreMultiplierCounter.MultiplierEventType ComputeNoteMultiplier(this NoteData.ScoringType scoringType)
        {
            return scoringType switch
            {
                NoteData.ScoringType.Ignore => ScoreMultiplierCounter.MultiplierEventType.Neutral,
                NoteData.ScoringType.NoScore => ScoreMultiplierCounter.MultiplierEventType.Neutral,
                _ => ScoreMultiplierCounter.MultiplierEventType.Positive,
            };
        }
        public static ScoreMultiplierCounter.MultiplierEventType ComputeNoteMultiplier(this NoteData noteData)
        {
            return ComputeNoteMultiplier(noteData.scoringType);
        }

        #endregion
    }
}