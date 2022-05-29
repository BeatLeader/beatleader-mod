using System.Reflection;

namespace BeatLeader.Replays.Scoring
{
    public class GoodCutScoringElementWithConstructor : GoodCutScoringElement
    {
        public GoodCutScoringElementWithConstructor(CutScoreBuffer buffer, ScoreMultiplierCounter.MultiplierEventType multiplierEventType, ScoreMultiplierCounter.MultiplierEventType wouldBeCorrectCutBestPossibleMultiplierEventType)
        {
            this.GetType().GetField("_cutScoreBuffer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, buffer);
            this.noteData = buffer.noteCutInfo.noteData;
            this._multiplierEventType = multiplierEventType;
            this._wouldBeCorrectCutBestPossibleMultiplierEventType = wouldBeCorrectCutBestPossibleMultiplierEventType;
            this.isFinished = true;
        }
    }
}
