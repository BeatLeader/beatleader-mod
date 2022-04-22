using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatLeader.Replays.Interfaces;
using BeatLeader.Replays.Tools;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleCutScoringElement : ScoringElement
    {
        public class Pool : Pool<SimpleCutScoringElement> { }

        public override ScoreMultiplierCounter.MultiplierEventType wouldBeCorrectCutBestPossibleMultiplierEventType => _wouldBeCorrectCutBestPossibleMultiplierEventType;
        public override ScoreMultiplierCounter.MultiplierEventType multiplierEventType => _multiplierEventType;

        protected ScoreMultiplierCounter.MultiplierEventType _multiplierEventType;
        protected ScoreMultiplierCounter.MultiplierEventType _wouldBeCorrectCutBestPossibleMultiplierEventType;

        protected SimpleCutScoreBufferWithComparator _cutScoreBuffer;
        protected override int executionOrder => cutScoreBuffer.noteScoreDefinition.executionOrder;
        public IReadonlyCutScoreBuffer cutScoreBuffer => _cutScoreBuffer;
        public override int cutScore => _cutScoreBuffer.cutScore;

        public virtual void Init(SimpleNoteCutComparator comparator)
        {
            noteData = comparator.noteController.noteData;
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

            _cutScoreBuffer = new SimpleCutScoreBufferWithComparator();
            _cutScoreBuffer.Init(comparator);
            isFinished = true;
        }
    }
}
