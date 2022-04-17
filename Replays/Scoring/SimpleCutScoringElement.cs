using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleCutScoringElement : ScoringElement, ICutScoreBufferDidFinishReceiver
    {
        public class Pool : Pool<SimpleCutScoringElement> { }

        public override ScoreMultiplierCounter.MultiplierEventType wouldBeCorrectCutBestPossibleMultiplierEventType => _wouldBeCorrectCutBestPossibleMultiplierEventType;
        public override ScoreMultiplierCounter.MultiplierEventType multiplierEventType => _multiplierEventType;

        protected ScoreMultiplierCounter.MultiplierEventType _multiplierEventType;
        protected ScoreMultiplierCounter.MultiplierEventType _wouldBeCorrectCutBestPossibleMultiplierEventType;

        protected SimpleCutScoreBuffer _cutScoreBuffer;

        public SimpleCutScoreBuffer cutScoreBuffer => _cutScoreBuffer;
        protected override int executionOrder => 100000;
        public override int cutScore => _cutScoreBuffer.cutScore;

        public virtual void Init(NoteCutInfo noteCutInfo)
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
            _cutScoreBuffer = new SimpleCutScoreBuffer();
            if (_cutScoreBuffer.Init(in noteCutInfo))
            {
                _cutScoreBuffer.RegisterDidFinishReceiver(this);
                isFinished = false;
            }
            else
            {
                isFinished = true;
            }
            _cutScoreBuffer.Refresh();
        }
        public virtual void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer)
        {
            isFinished = true;
            _cutScoreBuffer.UnregisterDidFinishReceiver(this);
        }
    }
}
