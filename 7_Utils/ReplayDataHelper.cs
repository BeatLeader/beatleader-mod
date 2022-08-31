using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using IPA.Utilities;
using BeatLeader.Models;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class ReplayDataHelper
    {
        static ReplayDataHelper()
        {
            normalEnvironmentType = Resources.FindObjectsOfTypeAll<EnvironmentTypeSO>()
                .FirstOrDefault(x => x.typeNameLocalizationKey == "NORMAL_ENVIRONMENT_TYPE");
        }

        private class ReplayModifiers : GameplayModifiers
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

        #region Environments

        public static readonly EnvironmentTypeSO normalEnvironmentType; 

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

        #region DataManagement

        public static GameplayModifiers GetModifiersFromReplay(this Replay replay)
        {
            ReplayModifiers replayModifiers = new ReplayModifiers();
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
        public static PracticeSettings GetPracticeSettingsFromReplay(this Replay replay)
        {
            return replay.info.startTime != 0 ? new PracticeSettings(replay.info.startTime, replay.info.speed) : null;
        }
        public static PlayerSpecificSettings ModifyPlayerSettingsByReplay(this PlayerSpecificSettings settings, Replay replay)
        {
            return settings.CopyWith(replay.info.leftHanded, replay.info.height, false);
        }
        public static StandardLevelScenesTransitionSetupDataSO CreateTransitionData(this Replay replay, PlayerDataModel playerModel, IDifficultyBeatmap difficulty, EnvironmentInfoSO environment = null)
        {
            var data = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            var playerData = playerModel.playerData;
            bool overrideEnv = environment != null;

            var environmentSettings = overrideEnv ? new OverrideEnvironmentSettings() : playerData.overrideEnvironmentSettings;
            if (overrideEnv)
            {
                environmentSettings.overrideEnvironments = true;
                environmentSettings.SetEnvironmentInfoForType(normalEnvironmentType, environment);
            }

            data?.Init("Solo", difficulty, difficulty.level, environmentSettings,
                playerData.colorSchemesSettings.GetOverrideColorScheme(),
                replay.GetModifiersFromReplay(),
                playerData.playerSpecificSettings.ModifyPlayerSettingsByReplay(replay),
                replay.GetPracticeSettingsFromReplay(), "Menu");

            return data;
        }
        public static NoteEvent GetNoteEvent(this NoteData noteData, Replay replay)
        {
            return GetNoteEventById(replay, noteData.ComputeNoteID(), noteData.time);
        }
        public static NoteEvent GetNoteEvent(this NoteController noteController, Replay replay)
        {
            return GetNoteEventById(replay, noteController.noteData.ComputeNoteID(), noteController.noteData.time);
        }
        public static WallEvent GetWallEvent(this ObstacleData obstacleData, Replay replay)
        {
            return GetWallEventById(replay, obstacleData.ComputeObstacleID(), obstacleData.time);
        }
        public static NoteEvent GetNoteEventById(this Replay replay, int ID, float time)
        {
            foreach (var item in replay.notes)
                if ((item.noteID == ID || item.noteID == ID - 30000) &&
                    (item.spawnTime >= time - 0.05f && item.spawnTime <= time + 0.15f))
                    return item;
            return null;
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
            return ReplayNoteCutInfo.Parse(noteEvent.noteCutInfo, noteController);
        }
        public static NoteCutInfo GetNoteCutInfo(this NoteController noteController, Replay replay)
        {
            var noteEvent = GetNoteEvent(noteController, replay);
            return noteEvent != null ? ReplayNoteCutInfo.Parse(noteEvent.noteCutInfo, noteController) : default;
        }
        public static LinkedListNode<Frame> GetFrameByTime(this LinkedList<Frame> frames, float time)
        {
            TryGetFrameByTime(frames, time, out var frame);
            return frame;
        }
        public static bool TryGetFrameByTime(this LinkedList<Frame> frames, float time, out LinkedListNode<Frame> frame)
        {
            frame = null;
            for (frame = frames.First; frame != null; frame = frame.Next)
                if (frame.Value.time >= time)
                    return true;
            return false;
        }
        public static Frame GetFrameByTime(this Replay replay, float time)
        {
            return GetFrameByTime(new LinkedList<Frame>(replay.frames), time).Value;
        }
        public static bool TryGetReplays(Func<Replay, bool> filter, out List<Replay> replays)
        {
            string[] replaysPaths = FileManager.GetAllReplaysPaths();
            replays = new List<Replay>();

            foreach (var path in replaysPaths)
            {
                if (!FileManager.TryReadReplay(path, out var extractedReplay)) continue;
                if (filter(extractedReplay)) replays.Add(extractedReplay);
            }

            return replays.Count >= 0;
        }

        #endregion

        #region Computing

        public static int ComputeObstacleID(this ObstacleData obstacleData)
        {
            return obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
        }
        public static int ComputeNoteID(this NoteData noteData)
        {
            return ((int)noteData.scoringType + 2) * 10000 + noteData.lineIndex * 1000 +
                   (int)noteData.noteLineLayer * 100 + (int)noteData.colorType * 10 + (int)noteData.cutDirection;
        }
        public static int ComputeNoteScore(this NoteEvent note)
        {
            if (note.eventType != NoteEventType.good)
                return note.eventType switch
                {
                    NoteEventType.bad => -2,
                    NoteEventType.miss => -3,
                    NoteEventType.bomb => -4,
                    _ => -1
                };

            var cut = note.noteCutInfo;
            double beforeCutRawScore = Mathf.Clamp((float)Math.Round(70 * cut.beforeCutRating), 0, 70);
            double afterCutRawScore = Mathf.Clamp((float)Math.Round(30 * cut.afterCutRating), 0, 30);
            double num = 1 - Mathf.Clamp(cut.cutDistanceToCenter / 0.3f, 0.0f, 1.0f);
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
            switch (scoringType)
            {
                default:
                    return ScoreMultiplierCounter.MultiplierEventType.Positive;
                case NoteData.ScoringType.Ignore:
                case NoteData.ScoringType.NoScore:
                    return ScoreMultiplierCounter.MultiplierEventType.Neutral;
            }
        }
        public static ScoreMultiplierCounter.MultiplierEventType ComputeNoteMultiplier(this NoteData noteData)
        {
            return ComputeNoteMultiplier(noteData.scoringType);
        }

        #endregion
    }
}