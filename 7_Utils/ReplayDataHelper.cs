using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using IPA.Utilities;
using BeatLeader.Models;
using BeatLeader.Utils;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class ReplayDataHelper
    {
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

        public enum Score
        {
            Best,
            Lowest,
            NA
        }
        public enum Result
        {
            Completed,
            Failed,
            NA
        }

        #region Environments
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
            return Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>().First(x => x.serializedName == GetEnvironmentSerializedNameByEnvironmentName(name));
        }
        #endregion

        #region Replaying
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
        public static PracticeSettings GetPracticeSettings(this Replay replay)
        {
            if (replay.info.startTime == 0) return null;
            else return new PracticeSettings(replay.info.startTime, replay.info.speed);
        }
        public static PlayerSpecificSettings OverridePlayerSettings(this PlayerSpecificSettings settings, Replay replay)
        {
            return settings.CopyWith(replay.info.leftHanded, replay.info.height, false);
        }
        public static StandardLevelScenesTransitionSetupDataSO CreateTransitionData(this Replay replay, PlayerDataModel playerModel, IDifficultyBeatmap difficulty, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            StandardLevelScenesTransitionSetupDataSO data = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();

            OverrideEnvironmentSettings environmentSettings = playerModel.playerData.overrideEnvironmentSettings;
            environmentSettings.SetEnvironmentInfoForType(Resources.FindObjectsOfTypeAll<EnvironmentTypeSO>().First(x => x.typeNameLocalizationKey == "NORMAL_ENVIRONMENT_TYPE"), ReplayDataHelper.GetEnvironmentByName(replay.info.environment));

            data.Init("Simple", difficulty, previewBeatmapLevel, environmentSettings,
                playerModel.playerData.colorSchemesSettings.GetOverrideColorScheme(),
                replay.GetModifiersFromReplay(), playerModel.playerData.playerSpecificSettings.OverridePlayerSettings(replay),
                replay.GetPracticeSettings(), "Menu");

            return data;
        }
        public static async Task<NoteEvent> GetNoteEventAsync(this NoteController noteController, Replay replay)
        {
            var noteEvent = await Task.Run(() => GetNoteEventByID(noteController.noteData.ComputeNoteID(), noteController.noteData.time, replay));
            return noteEvent;
        }
        public static async Task<NoteEvent> GetNoteEventAsync(this NoteData noteData, Replay replay)
        {
            var noteEvent = await Task.Run(() => GetNoteEventByID(noteData.ComputeNoteID(), noteData.time, replay));
            return noteEvent;
        }
        public static async Task<List<Replay>> TryGetReplaysBySongInfoAsync(this IDifficultyBeatmap data)
        {
            List<Replay> replays = await TryGetReplaysAsync((Replay replay) => replay.info.songName == data.level.songName && replay.info.difficulty == data.difficulty.ToString() && replay.info.mode == data.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName);
            if (replays != null)
            {
                return replays;
            }
            else
            {
                return null;
            }
        }
        public static async Task<List<Replay>> TryGetReplaysAsync(Func<Replay, bool> filter)
        {
            string[] replaysPaths = await Task.Run(() => FileManager.GetAllReplaysPaths());
            List<Replay> replays = new List<Replay>();

            foreach (var path in replaysPaths)
            {
                Replay extractedReplay = await Task.Run(() => FileManager.ReadReplay(path));
                if (filter(extractedReplay))
                {

                    replays.Add(extractedReplay);
                }
            }
            if (replays.Count > 0)
            {
                return replays;
            }
            else return null;
        }
        public static NoteEvent GetNoteEvent(this NoteData noteData, Replay replay)
        {
            return GetNoteEventByID(noteData.ComputeNoteID(), noteData.time, replay);
        }
        public static NoteEvent GetNoteEvent(this NoteController noteController, Replay replay)
        {
            return GetNoteEventByID(noteController.noteData.ComputeNoteID(), noteController.noteData.time, replay);
        }
        public static NoteEvent GetNoteEventByID(int ID, float time, Replay replay)
        {
            foreach (var item in replay.notes)
            {
                if ((item.noteID == ID || item.noteID == ID - 30000) && item.spawnTime == time)
                {
                    return item;
                }
            }
            return default;
        }
        public static List<NoteEvent> GetAllNoteEventsForNoteController(this NoteController controller, Replay replay)
        {
            List<NoteEvent> notes = new List<NoteEvent>();
            int ID = controller.noteData.ComputeNoteID();

            foreach (var item in replay.notes)
            {
                if (ID == item.noteID)
                {
                    notes.Add(item);
                }
            }
            return notes;
        }
        public static NoteCutInfo GetNoteCutInfo(this NoteController noteController, NoteEvent noteEvent)
        {
            return ReplayNoteCutInfo.Parse(noteEvent.noteCutInfo, noteController);
        }
        public static NoteCutInfo GetNoteCutInfo(this NoteController noteController, Replay replay)
        {
            var noteEvent = GetNoteEvent(noteController, replay);
            if (noteEvent != null)
            {
                return ReplayNoteCutInfo.Parse(noteEvent.noteCutInfo, noteController);
            }
            return default;
        }
        public static int GetFrameByTime(this Replay replay, float time, out Frame frame)
        {
            for (int i = 0; i < replay.frames.Count; i++)
            {
                frame = replay.frames[i];
                if (frame.time >= time && frame != null)
                {
                    return i;
                }
            }
            frame = null;
            return 0;
        }
        public static Frame GetFrameByTime(this Replay replay, float time)
        {
            for (int i = 0; i < replay.frames.Count; i++)
            {
                Frame frame = replay.frames[i];
                if (frame.time >= time && frame != null)
                {
                    return frame;
                }
            }
            return null;
        }
        public static Replay GetReplayWithScore(this List<Replay> replays, Score score)
        {
            int scoreIndex = -1;
            switch (score)
            {
                case Score.Best:
                    float bestScore = 0;
                    for (int i = 0; i < replays.Count; i++)
                    {
                        if (replays[i].info.score > bestScore)
                        {
                            bestScore = replays[i].info.score;
                            scoreIndex = i;
                        }
                    }
                    break;
                case Score.Lowest:
                    float lowestScore = float.MaxValue;
                    for (int i = 0; i < replays.Count; i++)
                    {
                        if (replays[i].info.score < lowestScore)
                        {
                            lowestScore = replays[i].info.score;
                            scoreIndex = i;
                        }
                    }
                    break;
            }
            if (replays.Count == 0) return null;
            return scoreIndex != -1 ? replays[scoreIndex] : replays[0];
        }
        public static bool TryGetReplaysBySongInfo(this IDifficultyBeatmap data, out List<Replay> replays)
        {
            bool condition = TryGetReplays(out List<Replay> replays2, (Replay replay) => replay.info.songName == data.level.songName && replay.info.difficulty == data.difficulty.ToString() && replay.info.mode == data.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName);
            if (condition)
            {
                replays = replays2;
                return true;
            }
            else
            {
                replays = null;
                return false;
            }
        }
        public static bool TryGetReplays(out List<Replay> replays, Func<Replay, bool> filter)
        {
            string[] replaysPaths = FileManager.GetAllReplaysPaths();
            replays = new List<Replay>();

            foreach (var path in replaysPaths)
            {
                Replay extractedReplay = FileManager.ReadReplay(path);
                if (filter(extractedReplay))
                {
                    replays.Add(extractedReplay);
                }
            }
            if (replays.Count <= 0)
            {
                return false;
            }
            else return true;
        }
        #endregion

        #region Computing
        public static int ComputeObstacleID(this ObstacleData obstacleData)
        {
            return obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
        }
        public static int ComputeNoteID(this NoteData noteData)
        {
            return ((int)noteData.scoringType + 2) * 10000 + noteData.lineIndex * 1000 + (int)noteData.noteLineLayer * 100 + (int)noteData.colorType * 10 + (int)noteData.cutDirection;
        }
        public static int ComputeNoteScore(this NoteEvent note)
        {
            if (note.eventType == NoteEventType.good)
            {
                var cut = note.noteCutInfo;
                double beforeCutRawScore = Mathf.Clamp((float)Math.Round(70 * cut.beforeCutRating), 0, 70);
                double afterCutRawScore = Mathf.Clamp((float)Math.Round(30 * cut.afterCutRating), 0, 30);
                double num = 1 - Mathf.Clamp(cut.cutDistanceToCenter / 0.3f, 0.0f, 1.0f);
                double cutDistanceRawScore = Math.Round(15 * num);
                return (int)beforeCutRawScore + (int)afterCutRawScore + (int)cutDistanceRawScore;
            }
            else
            {
                switch (note.eventType)
                {
                    case NoteEventType.bad:
                        return -2;
                    case NoteEventType.miss:
                        return -3;
                    case NoteEventType.bomb:
                        return -4;
                }
            }
            return -1;
        }
        #endregion
    }
}
