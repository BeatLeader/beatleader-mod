using System.Linq;
using BeatLeader.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Emulating
{
    public class BeatmapVisualsController : MonoBehaviour
    {
        [Inject] protected readonly BeatmapTimeController _beatmapTimeController;
        [Inject] protected readonly ComboController _comboController;
        [InjectOptional] protected readonly IComboInfoProvider _comboInfoProvider;

        protected ComboUIController _comboUIController;
        protected ObstacleSaberSparkleEffectManager _sparkleEffectManager;
        protected NoteDebrisSpawner _noteDebrisSpawner;
        protected SaberBurnMarkSparkles _saberBurnMarkSparkles;

        protected float _debrisCutDirMultiplier;
        protected float _debrisFromCenterSpeed;
        protected float _debrisMoveSpeedMultiplier;

        public void Start()
        {
            _comboUIController = Resources.FindObjectsOfTypeAll<ComboUIController>().First();
            _sparkleEffectManager = Resources.FindObjectsOfTypeAll<ObstacleSaberSparkleEffectManager>().First();
            _saberBurnMarkSparkles = Resources.FindObjectsOfTypeAll<SaberBurnMarkSparkles>().First();
            _noteDebrisSpawner = Resources.FindObjectsOfTypeAll<NoteDebrisSpawner>().First();
            _debrisCutDirMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_cutDirMultiplier");
            _debrisFromCenterSpeed = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_fromCenterSpeed");
            _debrisMoveSpeedMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_moveSpeedMultiplier");
            _beatmapTimeController.onSongTimeScale += AdjustDebrisPhysics;
            if (_comboInfoProvider != null)
                _comboInfoProvider.onComboChangedAfterRescoring += ForceSetCombo;
        }
        public void OnDestroy()
        {
            if (_beatmapTimeController != null)
                _beatmapTimeController.onSongTimeScale -= AdjustDebrisPhysics;
            if (_comboInfoProvider != null)
                _comboInfoProvider.onComboChangedAfterRescoring -= ForceSetCombo;
        }
        public void PauseEffects(bool pause)
        {
            _saberBurnMarkSparkles.enabled = !pause;
            _sparkleEffectManager.gameObject.SetActive(!pause);
            _sparkleEffectManager.GetField<ObstacleSaberSparkleEffect[], ObstacleSaberSparkleEffectManager>("_effects").ToList().ForEach(x => x.gameObject.SetActive(!pause));
        }
        public void EnableDebris(bool enable)
        {
            _noteDebrisSpawner.enabled = enable;
        }
        protected void ForceSetCombo(int combo, int maxCombo, bool broke)
        {
            _comboController.SetField("_combo", combo);
            _comboController.SetField("_maxCombo", maxCombo);
            _comboUIController.HandleComboDidChange(combo);
            Animator animator = _comboUIController.GetField<Animator, ComboUIController>("_animator");
            _comboUIController.SetField("_fullComboLost", false);
            if (broke)
            {
                _comboUIController.HandleComboBreakingEventHappened();
            }
            else
            {
                animator.enabled = false;
                animator.Rebind();
                animator.enabled = true;
            }
        }
        protected void AdjustDebrisPhysics(float multiplier)
        {
            _noteDebrisSpawner.SetField("_cutDirMultiplier", _debrisCutDirMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_moveSpeedMultiplier", _debrisMoveSpeedMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_fromCenterSpeed", _debrisFromCenterSpeed * multiplier);
        }
    }
}
