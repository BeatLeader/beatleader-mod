using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Emulating
{
    public class BeatmapEffectsController : MonoBehaviour
    {
        [Inject] protected readonly ObstacleSaberSparkleEffectManager _sparkleEffectManager;
        [Inject] protected readonly BeatmapTimeController _beatmapTimeController;

        protected NoteDebrisSpawner _noteDebrisSpawner;
        protected SaberBurnMarkSparkles _saberBurnMarkSparkles;

        protected float _debrisCutDirMultiplier;
        protected float _debrisFromCenterSpeed;
        protected float _debrisMoveSpeedMultiplier;

        public void Start()
        {
            _saberBurnMarkSparkles = Resources.FindObjectsOfTypeAll<SaberBurnMarkSparkles>().First();
            _noteDebrisSpawner = Resources.FindObjectsOfTypeAll<NoteDebrisSpawner>().First();
            _debrisCutDirMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_cutDirMultiplier");
            _debrisFromCenterSpeed = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_fromCenterSpeed");
            _debrisMoveSpeedMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_moveSpeedMultiplier");
            _beatmapTimeController.onSongTimeScale += AdjustDebrisPhysics;
        }
        public void OnDestroy()
        {
            _beatmapTimeController.onSongTimeScale -= AdjustDebrisPhysics;
        }
        public void PauseEffects(bool pause)
        {
            _saberBurnMarkSparkles.enabled = !pause;
            _sparkleEffectManager.gameObject.SetActive(!pause);
            _sparkleEffectManager.GetField<ObstacleSaberSparkleEffect[], ObstacleSaberSparkleEffectManager>("_effects").ToList().ForEach(x => x.gameObject.SetActive(!pause));
        }
        protected void AdjustDebrisPhysics(float multiplier)
        {
            _noteDebrisSpawner.SetField("_cutDirMultiplier", _debrisCutDirMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_moveSpeedMultiplier", _debrisMoveSpeedMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_fromCenterSpeed", _debrisFromCenterSpeed * multiplier);
        }
    }
}
