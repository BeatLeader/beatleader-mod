using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;
using BeatLeader.Replays.Interfaces;
using Zenject;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleScoringInterlayer
    {
        public class Pool : MemoryPool<SimpleScoringInterlayer>
        {
            protected override void Reinitialize(SimpleScoringInterlayer item)
            {
                item._isFinished = false;
                item._scoringElement = null;
            }
        }

        protected ScoringElement _scoringElement;
        protected bool _isFinished;

        public virtual ScoringElement scoringElement => _scoringElement;
        public bool isFinished => _isFinished;

        public virtual void Init(ScoringElement scoringElement)
        {
            SimpleCutScoringElement scoringElementWithInterlayer;
            if ((scoringElementWithInterlayer = (scoringElement as SimpleCutScoringElement)) != null)
            {
                ConvertScoringElement(scoringElementWithInterlayer, scoringElementWithInterlayer.cutScoreBuffer);
                _isFinished = true;
            }
        }
        public virtual void ConvertScoringElement(ScoringElement element, IReadonlyCutScoreBuffer buffer)
        {
            GoodCutScoringElement goodCutScoringElement = new GoodCutScoringElementWithConstructor(
                ConvertBuffer(buffer), element.multiplierEventType,
                element.wouldBeCorrectCutBestPossibleMultiplierEventType);
            _scoringElement = goodCutScoringElement;
        }
        private CutScoreBuffer ConvertBuffer(IReadonlyCutScoreBuffer buffer)
        {
            CutScoreBuffer cutScoreBuffer = new CutScoreBufferWithConstructor(buffer.noteCutInfo, buffer.noteScoreDefinition,
                buffer.afterCutScore, buffer.beforeCutScore, buffer.centerDistanceCutScore);
            return cutScoreBuffer;
        }
    }
}
