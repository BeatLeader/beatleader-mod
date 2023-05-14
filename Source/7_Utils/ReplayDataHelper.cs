using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using JetBrains.Annotations;
using UnityEngine;
using Replay = BeatLeader.Models.Replay.Replay;
using NoteEventType = BeatLeader.Models.Replay.NoteEventType;
using RNoteCutInfo = BeatLeader.Models.Replay.NoteCutInfo;

namespace BeatLeader.Utils {
    public static class ReplayDataHelper {
        #region Encoding

        class ReplayComparator : IReplayComparator {
            public bool Compare(NoteEvent noteEvent, NoteData noteData) => ReplayDataHelper.Compare(noteEvent, noteData);
        }

        public static readonly IReplayComparator BasicReplayComparator = new ReplayComparator();

        public static IReplay ConvertToAbstractReplay(Replay replay, Player? player) {
            var replayData = replay.info;
            var creplayData = new GenericReplayData(
                replayData.failTime,
                replayData.timestamp,
                replayData.leftHanded,
                replay.GetModifiersFromReplay(),
                replayData.jumpDistance,
                player, replay.GetPracticeSettingsFromReplay());

            var frames = replay.frames.Select(static x => new PlayerMovementFrame(
                x.time, new() {
                    position = (Vector3)x.head.position,
                    rotation = (Quaternion)x.head.rotation
                },
                new() {
                    position = (Vector3)x.leftHand.position,
                    rotation = (Quaternion)x.leftHand.rotation
                },
                new() {
                    position = (Vector3)x.rightHand.position,
                    rotation = (Quaternion)x.rightHand.rotation
                }
            ));

            var notes = replay.notes.Select(static x => new NoteEvent(
                x.noteID, x.eventTime, x.spawnTime, (NoteEvent.NoteEventType)x.eventType,
                x.noteCutInfo?.beforeCutRating ?? 0, x.noteCutInfo?.afterCutRating ?? 0,
                x.eventType == NoteEventType.bomb ? RNoteCutInfo.BombNoteCutInfo :
                x.noteCutInfo != null ? RNoteCutInfo.Convert(x.noteCutInfo) : default
            ));

            var walls = replay.walls.Select(static x =>
                new WallEvent(x.wallID, x.energy, x.time, x.spawnTime));

            var pauses = replay.pauses.Select(static x =>
                new PauseEvent(x.time, x.duration));

            return new GenericReplay(
                creplayData,
                frames.ToArray(),
                notes.ToArray(),
                walls.ToArray(),
                pauses.ToArray());
        }

        #endregion

        #region Data Management

        private static GameplayModifiers.SongSpeed SongSpeedFromModifiers(string modifiers) {
            if (modifiers.Contains("SS")) return GameplayModifiers.SongSpeed.Slower;
            if (modifiers.Contains("SF")) return GameplayModifiers.SongSpeed.SuperFast;
            if (modifiers.Contains("FS")) return GameplayModifiers.SongSpeed.Faster;

            return GameplayModifiers.SongSpeed.Normal;
        }

        private static GameplayModifiers GetModifiersFromReplay(this Replay replay) {
            var modifiers = replay.info.modifiers;

            return new(
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

        private static PracticeSettings? GetPracticeSettingsFromReplay(this Replay replay) {
            return replay.info.failTime == 0 ? null : new(replay.info.startTime,
                replay.info.speed is var speed && speed == 0 ? 1 : speed);
        }

        #endregion

        #region Computing

        [UsedImplicitly]
        public static int ComputeObstacleId(this ObstacleData obstacleData) {
            return obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
        }

        [UsedImplicitly]
        public static int ComputeNoteId(this NoteData noteData, bool noScoring = false, bool altBomb = false) {
            // Bombs may have both correct values as well as default.
            var colorType = altBomb && noteData.colorType == ColorType.None ? 0 : (int)noteData.colorType;
            var cutDirection = altBomb && noteData.colorType == ColorType.None ? 3 : (int)noteData.cutDirection;

            // Pre 1.20 replays has no scoring in ID
            var scoringPart = noScoring ? 0 : ((int)noteData.scoringType + 2) * 10000;

            return scoringPart + noteData.lineIndex
                * 1000 + (int)noteData.noteLineLayer * 100 + colorType
                * 10 + cutDirection;
        }

        [UsedImplicitly]
        public static bool Compare(NoteEvent noteEvent, NoteData noteData) {
            var id = noteEvent.noteId;
            return id == noteData.ComputeNoteId()
                || id == noteData.ComputeNoteId(true, false)
                || id == noteData.ComputeNoteId(true, true)
                || id == noteData.ComputeNoteId(false, true);
        }

        #endregion
    }
}