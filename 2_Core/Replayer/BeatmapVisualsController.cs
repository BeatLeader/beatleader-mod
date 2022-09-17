using System.Linq;
using BeatLeader.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class BeatmapVisualsController : MonoBehaviour
    {
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ComboController _comboController;
        [InjectOptional] private readonly IReplayerScoreController _scoreController;

        private ComboUIController _comboUIController;
        private ObstacleSaberSparkleEffectManager _sparkleEffectManager;
        private NoteDebrisSpawner _noteDebrisSpawner;
        private SaberBurnMarkSparkles _saberBurnMarkSparkles;

        private float _debrisCutDirMultiplier;
        private float _debrisFromCenterSpeed;
        private float _debrisMoveSpeedMultiplier;

        private void Start()
        {
            _comboUIController = Resources.FindObjectsOfTypeAll<ComboUIController>().First();
            _sparkleEffectManager = Resources.FindObjectsOfTypeAll<ObstacleSaberSparkleEffectManager>().First();
            _saberBurnMarkSparkles = Resources.FindObjectsOfTypeAll<SaberBurnMarkSparkles>().First();
            _noteDebrisSpawner = Resources.FindObjectsOfTypeAll<NoteDebrisSpawner>().First();
            _debrisCutDirMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_cutDirMultiplier");
            _debrisFromCenterSpeed = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_fromCenterSpeed");
            _debrisMoveSpeedMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_moveSpeedMultiplier");
            _beatmapTimeController.OnSongSpeedChanged += ModifyDebrisPhysics;
            if (_scoreController != null)
                _scoreController.OnComboChangedAfterRescoring += ForceSetCombo;
        }
        private void OnDestroy()
        {
            if (_beatmapTimeController != null)
                _beatmapTimeController.OnSongSpeedChanged -= ModifyDebrisPhysics;
            if (_scoreController != null)
                _scoreController.OnComboChangedAfterRescoring -= ForceSetCombo;
        }
        public void PauseEffects(bool pause)
        {
            _saberBurnMarkSparkles.enabled = !pause;
            _sparkleEffectManager.gameObject.SetActive(!pause);
            _sparkleEffectManager.GetField<ObstacleSaberSparkleEffect[], ObstacleSaberSparkleEffectManager>("_effects")
                .ToList().ForEach(x => x.gameObject.SetActive(!pause));
        }
        private void ForceSetCombo(int combo, int maxCombo, bool broke)
        {
            _comboController.SetField("_combo", combo);
            _comboController.SetField("_maxCombo", maxCombo);
            _comboUIController.HandleComboDidChange(combo);
            _comboUIController.SetField("_fullComboLost", false);
            if (broke)
                _comboUIController.HandleComboBreakingEventHappened();
            else
                _comboUIController.GetField<Animator, ComboUIController>("_animator").Rebind();
        }
        private void ModifyDebrisPhysics(float multiplier)
        {
            _noteDebrisSpawner.SetField("_cutDirMultiplier", _debrisCutDirMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_moveSpeedMultiplier", _debrisMoveSpeedMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_fromCenterSpeed", _debrisFromCenterSpeed * multiplier);
        }
    }
}
