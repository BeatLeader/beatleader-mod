using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;
using BeatLeader.Replays.Scoring;

namespace BeatLeader.Replays.Tools
{
    public class SimpleScoringInterlayer
    {
        public virtual GoodCutScoringElement ConvertScoringElement(ScoringElement element, IReadonlyCutScoreBuffer buffer)
        {
            GoodCutScoringElement goodCutScoringElement = new GoodCutScoringElement();
            goodCutScoringElement.SetField("_multiplierEventType", element.multiplierEventType);
            goodCutScoringElement.SetField("_wouldBeCorrectCutBestPossibleMultiplierEventType", element.wouldBeCorrectCutBestPossibleMultiplierEventType);
            goodCutScoringElement.SetField("_cutScoreBuffer", ConvertBufferForcibly(buffer));
            return goodCutScoringElement;
        }
        public virtual GoodCutScoringElement ConvertScoringElement(NoteEventCutScoringElement element)
        {
            GoodCutScoringElement goodCutScoringElement = new GoodCutScoringElement();
            goodCutScoringElement.SetField("_multiplierEventType", element.multiplierEventType);
            goodCutScoringElement.SetField("_wouldBeCorrectCutBestPossibleMultiplierEventType", element.wouldBeCorrectCutBestPossibleMultiplierEventType);
            goodCutScoringElement.SetField("_cutScoreBuffer", ConvertBufferForcibly(element.cutScoreBuffer));
            goodCutScoringElement.GetType().GetProperty("noteData").SetValue(goodCutScoringElement, element.noteData);
            goodCutScoringElement.GetType().GetProperty("isFinished").SetValue(goodCutScoringElement, true);
            Debug.LogWarning(goodCutScoringElement);
            return goodCutScoringElement;
        }
        private CutScoreBuffer ConvertBufferForcibly(IReadonlyCutScoreBuffer buffer)
        {
            CutScoreBuffer cutScoreBuffer = new CutScoreBuffer();
            cutScoreBuffer.SetField("_noteCutInfo", buffer.noteCutInfo); 
            cutScoreBuffer.SetField("_noteScoreDefinition", buffer.noteScoreDefinition); 
            cutScoreBuffer.SetField("_afterCutScore", buffer.afterCutScore); 
            cutScoreBuffer.SetField("_beforeCutScore", buffer.beforeCutScore); 
            cutScoreBuffer.SetField("_centerDistanceCutScore", buffer.centerDistanceCutScore); 
            cutScoreBuffer.SetField("_isFinished", buffer.isFinished); 
            return cutScoreBuffer;
        }
    }
}
