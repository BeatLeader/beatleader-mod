using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BeatLeader.Replays.Models.Replay;
using UnityEngine;

namespace BeatLeader.Replays.Emulators
{
    public class ReplayCutScoringElement : ScoringElement
    {
        public class Pool : Pool<ReplayCutScoringElement> { }
        public override ScoreMultiplierCounter.MultiplierEventType wouldBeCorrectCutBestPossibleMultiplierEventType => _wouldBeCorrectCutBestPossibleMultiplierEventType;
        public override ScoreMultiplierCounter.MultiplierEventType multiplierEventType => _multiplierEventType;
        public override int cutScore => _cutScore;

        protected ScoreMultiplierCounter.MultiplierEventType _multiplierEventType;
        protected ScoreMultiplierCounter.MultiplierEventType _wouldBeCorrectCutBestPossibleMultiplierEventType;
        private protected int _cutScore;

        protected override int executionOrder => 100000;
        public virtual void Init(NoteCutInfo noteCutInfo, NoteEvent noteEvent)
        {
            noteData = noteCutInfo.noteData;
            switch (noteData.scoringType)
            {
                case NoteData.ScoringType.Ignore:
                    _multiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Neutral;
                    _wouldBeCorrectCutBestPossibleMultiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Neutral;
                    break;
                case NoteData.ScoringType.Normal:
                case NoteData.ScoringType.SliderHead:
                case NoteData.ScoringType.SliderTail:
                case NoteData.ScoringType.BurstSliderHead:
                case NoteData.ScoringType.BurstSliderElement:
                    _multiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Positive;
                    _wouldBeCorrectCutBestPossibleMultiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Positive;
                    break;
                case NoteData.ScoringType.NoScore:
                    _multiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Neutral;
                    _wouldBeCorrectCutBestPossibleMultiplierEventType = ScoreMultiplierCounter.MultiplierEventType.Neutral;
                    break;
            }
            _cutScore = noteEvent.ComputeNoteScore();
            isFinished = true;
        }
    }
}
