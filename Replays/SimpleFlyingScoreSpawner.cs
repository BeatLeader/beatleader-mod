using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class SimpleFlyingScoreSpawner : MonoBehaviour, IFlyingObjectEffectDidFinishEvent
    {
        [Inject] protected readonly FlyingScoreEffect.Pool _flyingScoreEffectPool;
        [Inject] protected readonly FlyingScoreSpawner.InitData _initData;

        public virtual void SpawnFlyingScore(Quaternion worldRotation, Quaternion inverseWorldRotation, Vector3 cutPoint, int score, Color color)
        {
            FlyingScoreEffect flyingScoreEffect = _flyingScoreEffectPool.Spawn();
            flyingScoreEffect.didFinishEvent.Add(this);
            flyingScoreEffect.transform.localPosition = cutPoint;
            Vector3 vector = cutPoint;
            vector = inverseWorldRotation * vector;
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
            Vector3 targetPos = worldRotation * (vector + new Vector3(0f, y, 7.55f));
            InitAndPresentFlyingScoreEffect(flyingScoreEffect, score, 115, 0.7f, targetPos, worldRotation, color);
        }
        public void InitAndPresentFlyingScoreEffect(FlyingScoreEffect flyingScoreEffect, int score, int maxPossibleScore, float duration, Vector3 targetPos, Quaternion rot, Color color)
        {
            //bool enabled = cutScoreBuffer.centerDistanceCutScore == cutScoreBuffer.noteScoreDefinition.maxCenterDistanceCutScore;
            //var indicator = flyingScoreEffect.GetField<SpriteRenderer, FlyingScoreEffect>("_maxCutDistanceScoreIndicator"); 
            //indicator.enabled = enabled;
            flyingScoreEffect.SetField("_color", color);
            flyingScoreEffect.SetField("_colorAMultiplier", (((float)score > (float)maxPossibleScore * 0.9f) ? 1f : 0.3f));
            flyingScoreEffect.RefreshScore(score, 115);
            flyingScoreEffect.InitAndPresent(0.7f, targetPos, rot, false);
        }
        public virtual void HandleFlyingObjectEffectDidFinish(FlyingObjectEffect flyingObjectEffect)
        {
            flyingObjectEffect.didFinishEvent.Remove(this);
            _flyingScoreEffectPool.Despawn(flyingObjectEffect as FlyingScoreEffect);
        }
    }
}
