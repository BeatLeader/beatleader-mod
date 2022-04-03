using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatLeader.Replays.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Replays
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

        #region Environment
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
        public static void ModifyEnrivonmentDataByReplay(this EnvironmentInfoSO info, Replay replay)
        {
            info.SetField("_environmentName", replay.info.environment);
            info.SetField("_serializedName", GetEnvironmentSerializedNameByEnvironmentName(replay.info.environment));
            info.sceneInfo.SetField("_sceneName", replay.info.environment);
        }
        #endregion

        public static bool TryGetReplayBySongName(string name, Result replayResult, Score replayScore, out Replay replay)
        {
            string[] replaysPaths = Directory.GetFiles(FileManager.replayFolderPath);
            List<Replay> matchedReplays = new List<Replay>();
            List<int> cellsToRemove = new List<int>();
            replay = null;

            foreach (var path in replaysPaths)
            {
                Replay extractedReplay = FileManager.ReadReplay(path);
                if (extractedReplay.info.songName == name)
                    matchedReplays.Add(extractedReplay);
            }
            if (matchedReplays.Count == 0) return false;
            switch (replayResult)
            {
                case Result.Completed:
                    for (int i = 0; i < matchedReplays.Count; i++)
                    {
                        if (matchedReplays[i].info.failTime != 0)
                        {
                            cellsToRemove.Add(i);
                        }
                    }
                    break;
                case Result.Failed:
                    for (int i = 0; i < matchedReplays.Count; i++)
                    {
                        if (matchedReplays[i].info.failTime == 0)
                        {
                            cellsToRemove.Add(i);
                        }
                    }
                    break;
            }

            foreach (int index in cellsToRemove)
            {
                matchedReplays.RemoveAt(index);
            }
            cellsToRemove.Clear();
            if (matchedReplays.Count == 0) return false;

            int scoreIndex = -1;
            switch (replayScore)
            {
                case Score.Best:
                    float bestScore = 0;
                    for (int i = 0; i < matchedReplays.Count; i++)
                    {
                        if (matchedReplays[i].info.score > bestScore)
                        {
                            bestScore = matchedReplays[i].info.score;
                            scoreIndex = i;
                        }
                    }
                    break;
                case Score.Lowest:
                    float lowestScore = float.MaxValue;
                    for (int i = 0; i < matchedReplays.Count; i++)
                    {
                        if (matchedReplays[i].info.score < lowestScore)
                        {
                            lowestScore = matchedReplays[i].info.score;
                            scoreIndex = i;
                        }
                    }
                    break;
            }
            if (matchedReplays.Count == 0) return false;

            replay = scoreIndex != -1 ? matchedReplays[scoreIndex] : matchedReplays[0];
            return true;
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
    }
}
