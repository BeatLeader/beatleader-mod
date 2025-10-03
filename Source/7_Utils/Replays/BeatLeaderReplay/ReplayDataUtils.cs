using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using JetBrains.Annotations;
using UnityEngine;
using Replay = BeatLeader.Models.Replay.Replay;
using RNoteEventType = BeatLeader.Models.Replay.NoteEventType;
using RNoteCutInfo = BeatLeader.Models.Replay.NoteCutInfo;

namespace BeatLeader.Utils {
    [PublicAPI]
    public static class ReplayDataUtils {
        #region Encoding

        private class ReplayNoteComparator : IReplayNoteComparator {
            bool IReplayNoteComparator.Compare(NoteEvent noteEvent, NoteData noteData) {
                return ReplayDataUtils.Compare(noteEvent, noteData.ToLightData());
            }
        }

        public static readonly IReplayNoteComparator BasicReplayNoteComparator = new ReplayNoteComparator();

        public static IReplay ConvertToAbstractReplay(Replay replay, IPlayer? player, BattleRoyaleReplayData? optionalData, bool mirrorX) {
            var replayData = replay.info;
            var failed = replayData.failTime is not 0;
            var creplayData = new GenericReplayData(
                failed ? replayData.failTime : replay.frames.LastOrDefault()?.time ?? 0,
                failed ? ReplayFinishType.Failed : ReplayFinishType.Cleared,
                int.Parse(replayData.timestamp),
                replayData.leftHanded,
                replayData.jumpDistance,
                replay.heights.Count == 0 ? replayData.height : null,
                replay.GetModifiersFromReplay(),
                player,
                replay.info.GetPracticeSettingsFromInfo()
            );

            var frames = replay.frames.Select(x => {
                    var frame = new PlayerMovementFrame(
                        x.time,
                        new() {
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
                    );

                    return mirrorX ? frame.MirrorX() : frame;
                }
            );

            var notes = replay.notes.Select(x => {
                    var evt = new NoteEvent(
                        x.noteID,
                        x.eventTime,
                        x.spawnTime,
                        (NoteEvent.NoteEventType)x.eventType,
                        x.noteCutInfo?.beforeCutRating ?? 0,
                        x.noteCutInfo?.afterCutRating ?? 0,
                        x.eventType == RNoteEventType.bomb ? RNoteCutInfo.BombNoteCutInfo :
                        x.noteCutInfo != null ? RNoteCutInfo.Convert(x.noteCutInfo) : default
                    );

                    return mirrorX ? evt : evt.MirrorX();
                }
            );

            var walls = replay.walls.Select(static x => new WallEvent(x.wallID, x.energy, x.time, x.spawnTime));
            var pauses = replay.pauses.Select(static x => new PauseEvent(x.time, x.duration));

            var heights = replay.heights.Count is 0 ? null :
                replay.heights.Select(static x => new HeightEvent(x.time, x.height));

            // TODO: add proper support
            var comparator = mirrorX ? BasicReplayNoteComparator : BasicReplayNoteComparator;

            return new GenericReplay(
                creplayData,
                comparator,
                optionalData,
                frames.ToArray(),
                notes.ToArray(),
                walls.ToArray(),
                pauses.ToArray(),
                heights?.ToArray(),
                replay.customData
            );
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
                smallCubes: modifiers.Contains("SC")
            );
        }

        private static PracticeSettings? GetPracticeSettingsFromInfo(this Models.Replay.ReplayInfo info) {
            return info.failTime != 0 || info.startTime != 0 || info.speed != 0 ? new(
                info.startTime,
                info.speed is var speed && speed == 0 ? 1 : speed
            ) : null;
        }

        #endregion

        #region Computing

        public static int ComputeObstacleId(this ObstacleData obstacleData) {
            return obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
        }

        public static int ComputeNoteId(
            this in LightNoteData noteData,
            bool noScoring = false,
            bool altScoring = false,
            bool legacyScoring = false
        ) {
            // Bombs may have both correct values as well as default.
            var colorType = altScoring && noteData.colorType == ColorType.None ? 3 : (int)noteData.colorType;

            // Pre 1.20 replays has no scoring in ID
            var scoringPart = noScoring ? 0 : ((int)noteData.scoringType + 2) * 10000;

            // Pre 1.40 replays has no V3 combination scoring
            if (legacyScoring) {
                switch (noteData.scoringType) {
                    case NoteData.ScoringType.ArcHeadArcTail:
                        scoringPart = ((int)NoteData.ScoringType.ArcTail + 2) * 10000;
                        break;
                    case NoteData.ScoringType.ChainHeadArcTail:
                        scoringPart = ((int)NoteData.ScoringType.ChainHead + 2) * 10000;
                        break;
                    case NoteData.ScoringType.ChainLinkArcHead:
                        scoringPart = ((int)NoteData.ScoringType.ChainLink + 2) * 10000;
                        break;
                    default:
                        break;
                }
            }

            // Notes that are both chain and arc head have undefined scoring type priority
            // Bools are reused for compactness
            if (legacyScoring) {
                if (noteData.scoringType == NoteData.ScoringType.ChainHead && noteData.isArcHead) {
                    scoringPart = ((int)NoteData.ScoringType.ArcHead + 2) * 10000;
                }
            } else if (altScoring) {
                if (noteData.scoringType == NoteData.ScoringType.ArcHead && noteData.gameplayType == NoteData.GameplayType.BurstSliderHead) {
                    scoringPart = ((int)NoteData.ScoringType.ChainHead + 2) * 10000;
                }
            }

            return scoringPart
                + noteData.lineIndex * 1000
                + (int)noteData.noteLineLayer * 100
                + colorType * 10
                + (int)noteData.cutDirection;
        }

        public static bool Compare(NoteEvent noteEvent, in LightNoteData noteData) {
            var id = noteEvent.noteId;
            return id == noteData.ComputeNoteId()
                || id == noteData.ComputeNoteId(true, false)
                || id == noteData.ComputeNoteId(true, true)
                || id == noteData.ComputeNoteId(false, true)
                || id == noteData.ComputeNoteId(false, false, true);
        }

        public static LightNoteData ToLightData(this NoteData data) {
            return new LightNoteData(
                data.colorType,
                data.scoringType,
                data.gameplayType,
                data.isArcHead,
                data.isArcTail,
                data.lineIndex,
                data.noteLineLayer,
                data.cutDirection
            );
        }

        #endregion
    }
}