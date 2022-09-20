using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Replayer {
    internal class NoteControllerEmulator : NoteController {
        #region Setup

        private NoteData _noteData;
        public override NoteData noteData => _noteData;
        public NoteCutInfo CutInfo { get; private set; }

        public void Setup(NoteEvent noteEvent) {
            _noteData = CreateNoteData(noteEvent);
            CutInfo = CreateNoteCutInfo(noteEvent, _noteData);
        }

        #endregion

        #region CreateNoteCutInfo

        private static readonly SaberMovementData emptySaberMovementData = new();
        private static readonly Models.NoteCutInfo emptyCutInfo = new();

        private static NoteCutInfo CreateNoteCutInfo(NoteEvent noteEvent, NoteData noteData) {
            var i = noteEvent?.noteCutInfo ?? emptyCutInfo;

            return new NoteCutInfo(
                noteData,
                i.speedOK,
                i.directionOK,
                i.saberTypeOK,
                i.wasCutTooSoon,
                i.saberSpeed,
                i.saberDir,
                (SaberType)i.saberType,
                i.timeDeviation,
                i.cutDirDeviation,
                i.cutPoint,
                i.cutNormal,
                i.cutDistanceToCenter,
                i.cutAngle,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Vector3.zero,
                emptySaberMovementData
            );
        }

        #endregion

        #region CreateNoteData

        private static readonly NoteData emptyNoteData = NoteData.CreateBasicNoteData(0, 0, NoteLineLayer.Base, ColorType.ColorA, NoteCutDirection.Any);

        private static NoteData CreateNoteData(NoteEvent noteEvent) {
            ReplayDataHelper.DecodeNoteId(
                noteEvent.noteID,
                out var scoringType,
                out var lineIndex,
                out var noteLineLayer,
                out var colorType,
                out var cutDirection
            );

            var gameplayType = scoringType switch {
                NoteData.ScoringType.Ignore => NoteData.GameplayType.Normal,
                NoteData.ScoringType.NoScore => NoteData.GameplayType.Bomb,
                NoteData.ScoringType.Normal => NoteData.GameplayType.Normal,
                NoteData.ScoringType.SliderHead => NoteData.GameplayType.Normal,
                NoteData.ScoringType.SliderTail => NoteData.GameplayType.Normal,
                NoteData.ScoringType.BurstSliderHead => NoteData.GameplayType.BurstSliderHead,
                NoteData.ScoringType.BurstSliderElement => NoteData.GameplayType.BurstSliderElement,
                _ => throw new ArgumentOutOfRangeException()
            };

            return emptyNoteData.CopyWith(
                time: noteEvent.spawnTime,
                lineIndex: lineIndex,
                noteLineLayer: noteLineLayer,
                gameplayType: gameplayType,
                scoringType: scoringType,
                colorType: colorType,
                cutDirection: cutDirection
            );
        }

        #endregion

        #region Garbage

        protected override void HiddenStateDidChange(bool _) { }
        public override void Pause(bool _) { }

        #endregion
    }
}