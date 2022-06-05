using System;
using BeatLeader.Replays.Models;
using UnityEngine;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleScoringInterlayer : IScoringInterlayer
    {
        public ScoringElement Convert(ScoringElement element, Type type)
        {
            if (type != typeof(GoodCutScoringElement)) return null;

            SimpleCutScoringElement scoringElementWithInterlayer;
            if ((scoringElementWithInterlayer = (element as SimpleCutScoringElement)) != null)
            {
                GoodCutScoringElement goodCutScoringElement = new GoodCutScoringElementWithConstructor(
                    ConvertBuffer(scoringElementWithInterlayer.cutScoreBuffer), 
                    element.multiplierEventType, element.wouldBeCorrectCutBestPossibleMultiplierEventType);
                return goodCutScoringElement;
            }

            return null;
        }
        private CutScoreBuffer ConvertBuffer(IReadonlyCutScoreBuffer buffer)
        {
            return new CutScoreBufferWithConstructor(buffer.noteCutInfo, buffer.noteScoreDefinition,
                buffer.afterCutScore, buffer.beforeCutScore, buffer.centerDistanceCutScore);
        }
    }
}
