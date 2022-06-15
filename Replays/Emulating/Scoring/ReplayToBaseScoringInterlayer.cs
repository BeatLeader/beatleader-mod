using System;
using BeatLeader.Replays.Models;
using UnityEngine;

namespace BeatLeader.Replays.Scoring
{
    public class ReplayToBaseScoringInterlayer : IScoringInterlayer
    {
        public T Convert<T>(ScoringElement element) where T : ScoringElement
        {
            if (typeof(T) != typeof(GoodCutScoringElement)) return default;

            SimpleCutScoringElement scoringElementWithInterlayer;
            if ((scoringElementWithInterlayer = (element as SimpleCutScoringElement)) != null)
            {
                GoodCutScoringElement goodCutScoringElement = new GoodCutScoringElementWithConstructor(
                    ConvertBuffer(scoringElementWithInterlayer.cutScoreBuffer), 
                    element.multiplierEventType, element.wouldBeCorrectCutBestPossibleMultiplierEventType);
                return (T)(ScoringElement)goodCutScoringElement;
            }

            return default;
        }
        private CutScoreBuffer ConvertBuffer(IReadonlyCutScoreBuffer buffer)
        {
            return new CutScoreBufferWithConstructor(buffer.noteCutInfo, buffer.noteScoreDefinition,
                buffer.afterCutScore, buffer.beforeCutScore, buffer.centerDistanceCutScore);
        }
    }
}
