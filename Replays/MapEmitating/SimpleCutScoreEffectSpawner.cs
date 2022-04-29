using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Scoring;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.MapEmitating
{
    public class SimpleCutScoreEffectSpawner : MonoBehaviour, IFlyingObjectEffectDidFinishEvent
    {
        [Inject] protected readonly FlyingScoreEffect.Pool _flyingScoreEffectPool;
        [Inject] protected readonly FlyingScoreSpawner.InitData _initData;
        [Inject] protected readonly IScoreController _scoreController;

        public void Start()
        {
            _scoreController.scoringForNoteFinishedEvent += HandleScoringForNoteStarted;
        }
        public void OnDestroy()
        {
            _scoreController.scoringForNoteFinishedEvent -= HandleScoringForNoteStarted;
        }
        public virtual void SpawnFlyingScore(IReadonlyCutScoreBuffer cutScoreBuffer, Color color)
        {
            NoteCutInfo noteCutInfo = cutScoreBuffer.noteCutInfo;
            Vector3 vector = noteCutInfo.cutPoint;
            FlyingScoreEffect flyingScoreEffect = _flyingScoreEffectPool.Spawn();
            flyingScoreEffect.didFinishEvent.Add(this);
            flyingScoreEffect.transform.localPosition = vector;
            vector = noteCutInfo.inverseWorldRotation * vector;
            vector.z = 0f;
            float y = 0f;
            if (_initData.spawnPosition == FlyingScoreSpawner.SpawnPosition.Underground)
            {
                vector.y = -0.24f;
            }
            else
            {
                vector.y = 0.25f;
                y = -0.1f;
            }

            Vector3 targetPos = noteCutInfo.worldRotation * (vector + new Vector3(0f, y, 7.55f));
            flyingScoreEffect.InitAndPresent(cutScoreBuffer, 0.7f, targetPos, color);
        }
        public virtual void HandleFlyingObjectEffectDidFinish(FlyingObjectEffect flyingObjectEffect)
        {
            flyingObjectEffect.didFinishEvent.Remove(this);
            _flyingScoreEffectPool.Despawn(flyingObjectEffect as FlyingScoreEffect);
        }
        public virtual void HandleScoringForNoteStarted(ScoringElement scoringElement)
        {
            GoodCutScoringElement goodCutScoringElement;
            SimpleCutScoringElement simpleCutScoringElement;

            if ((goodCutScoringElement = (scoringElement as GoodCutScoringElement)) != null)
            {
                SpawnFlyingScore(goodCutScoringElement.cutScoreBuffer, new Color(0.8f, 0.8f, 0.8f));
            }
            else if ((simpleCutScoringElement = (scoringElement as SimpleCutScoringElement)) != null)
            {
                SpawnFlyingScore(simpleCutScoringElement.cutScoreBuffer, new Color(0.8f, 0.8f, 0.8f));
            }
        }
    }
}
