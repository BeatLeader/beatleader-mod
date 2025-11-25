using System;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;
using static NoteData;

namespace BeatLeader.Utils {
    internal static class AbstractReplayUtils {
        #region Other

        public static PlayerSpecificSettings GetPlayerSettingsByReplay(this PlayerSpecificSettings settings, IReplay replay) {
            return settings.CopyWith(replay.ReplayData.LeftHanded, replay.ReplayData.FixedHeight, replay.ReplayData.FixedHeight is null);
        }

        #endregion

        #region Playback

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

        #endregion

        #region Calculation

        public static int CalculateScoreForNote(NoteEvent note, ScoringType scoringType) {
            if (note.eventType != NoteEvent.NoteEventType.GoodCut) {
                return note.eventType switch {
                    NoteEvent.NoteEventType.BadCut  => -2,
                    NoteEvent.NoteEventType.Miss    => -3,
                    NoteEvent.NoteEventType.BombCut => -4,
                    _                               => -1
                };
            }

            var (before, after, acc) = CalculateCutScoresForNote(note, scoringType);
            return before + after + acc;
        }

        public static (int, int, int) CalculateCutScoresForNote(NoteEvent note, ScoringType scoringType) {
            var cut = note.noteCutInfo;
            double beforeCutRawScore = 0;
            if (scoringType != ScoringType.ChainLink) {
                beforeCutRawScore = scoringType switch {
                    ScoringType.ArcTail => 70,
                    _                   => Mathf.Clamp(Mathf.Round(70 * note.beforeCutRating), 0, 70)
                };
            }
            double afterCutRawScore = 0;
            if (scoringType != ScoringType.ChainLink) {
                afterCutRawScore = scoringType switch {
                    ScoringType.ChainHead => 0,
                    ScoringType.ArcHead   => 30,
                    _                     => Mathf.Clamp(Mathf.Round(30 * note.afterCutRating), 0, 30)
                };
            }
            double cutDistanceRawScore;
            if (scoringType == ScoringType.ChainLink) {
                cutDistanceRawScore = 20;
            } else {
                double num = 1 - Mathf.Clamp01(cut.cutDistanceToCenter / 0.3f);
                cutDistanceRawScore = Math.Round(15 * num);
            }
            return ((int)beforeCutRawScore, (int)afterCutRawScore, (int)cutDistanceRawScore);
        }

        #endregion
    }
}