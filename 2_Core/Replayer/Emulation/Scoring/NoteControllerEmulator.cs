using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Replayer.Emulation
{
    public class NoteControllerEmulator : NoteController
    {
        static NoteControllerEmulator()
        {
            emptyNoteData = NoteData.CreateBasicNoteData(0, 0, 
                NoteLineLayer.Base, ColorType.ColorA, NoteCutDirection.Any);
            emptyNoteCutInfo = new();
        }

        public override NoteData noteData => _noteData;
        public NoteCutInfo CutInfo { get; private set; }

        private NoteData _noteData;

        public void Setup(NoteEvent noteEvent)
        {
            _noteData = CreateNoteData(noteEvent);
            CutInfo = Models.NoteCutInfo.Convert(noteEvent?.noteCutInfo ?? emptyNoteCutInfo, _noteData);
        }

        private static readonly NoteData emptyNoteData;
        private static readonly Models.NoteCutInfo emptyNoteCutInfo;

        private static NoteData CreateNoteData(NoteEvent noteEvent)
        {
            ReplayDataHelper.DecodeNoteId(
                noteEvent.noteID,
                out var scoringType,
                out var lineIndex,
                out var noteLineLayer,
                out var colorType,
                out var cutDirection
            );

            var gameplayType = scoringType switch
            {
                NoteData.ScoringType.Ignore => NoteData.GameplayType.Normal,
                NoteData.ScoringType.NoScore => NoteData.GameplayType.Bomb,
                NoteData.ScoringType.Normal => NoteData.GameplayType.Normal,
                NoteData.ScoringType.SliderHead => NoteData.GameplayType.Normal,
                NoteData.ScoringType.SliderTail => NoteData.GameplayType.Normal,
                NoteData.ScoringType.BurstSliderHead => NoteData.GameplayType.BurstSliderHead,
                NoteData.ScoringType.BurstSliderElement => NoteData.GameplayType.BurstSliderElement,
                _ => NoteData.GameplayType.Normal
            };
            Debug.Log(gameplayType);

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

        #region Garbage

        protected override void HiddenStateDidChange(bool _) { }
        public override void Pause(bool _) { }

        #endregion
    }
}