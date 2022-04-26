using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BeatLeader.Replays.Scoring
{
    public class GoodCutScoringElementWithConstructor : GoodCutScoringElement
    {
        public GoodCutScoringElementWithConstructor(CutScoreBuffer buffer, ScoreMultiplierCounter.MultiplierEventType multiplierEventType, ScoreMultiplierCounter.MultiplierEventType wouldBeCorrectCutBestPossibleMultiplierEventType) : base()
        {
            this.GetType().GetField("_cutScoreBuffer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, buffer);
            this.noteData = buffer.noteCutInfo.noteData;
            this._multiplierEventType = multiplierEventType;
            this._wouldBeCorrectCutBestPossibleMultiplierEventType = wouldBeCorrectCutBestPossibleMultiplierEventType;
            this.isFinished = true;
        }
    }
}
