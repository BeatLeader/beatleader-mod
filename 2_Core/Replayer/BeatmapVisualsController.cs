using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class BeatmapVisualsController : MonoBehaviour
    {
        #region Injection

        [Inject] protected readonly PlaybackController _playbackController;
        [Inject] protected readonly BeatmapTimeController _beatmapTimeController;
        [Inject] protected readonly ComboController _comboController;
        [Inject] protected readonly ReplayEventsProcessor _eventsProcessor;

        [FirstResource] protected ComboUIController _comboUIController;
        [FirstResource] protected ObstacleSaberSparkleEffectManager _sparkleEffectManager;
        [FirstResource] protected NoteDebrisSpawner _noteDebrisSpawner;
        [FirstResource] protected SaberBurnMarkSparkles _saberBurnMarkSparkles;

        #endregion

        #region Setup

        protected float _debrisCutDirMultiplier;
        protected float _debrisFromCenterSpeed;
        protected float _debrisMoveSpeedMultiplier;
        private bool _comboWasBroke;
        private bool _wasInProcess;

        private void Awake()
        {
            this.LoadResources();
            _debrisCutDirMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_cutDirMultiplier");
            _debrisFromCenterSpeed = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_fromCenterSpeed");
            _debrisMoveSpeedMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_moveSpeedMultiplier");

            _playbackController.PauseStateChangedEvent += HandlePauseStateChanged;
            _beatmapTimeController.SongSpeedChangedEvent += HandleSongSpeedChanged;
            _eventsProcessor.ReprocessRequestedEvent += HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent += HandleReprocessDone;
            _comboController.comboBreakingEventHappenedEvent += HandleComboDidBreak;
        }
        private void OnDestroy()
        {
            _playbackController.PauseStateChangedEvent -= HandlePauseStateChanged;
            _beatmapTimeController.SongSpeedChangedEvent -= HandleSongSpeedChanged;
            _eventsProcessor.ReprocessRequestedEvent -= HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent -= HandleReprocessDone;
            _comboController.comboBreakingEventHappenedEvent -= HandleComboDidBreak;
        }

        #endregion

        #region Visuals control

        public void PauseSabersSparkles(bool pause)
        {
            _saberBurnMarkSparkles.enabled = !pause;
            _sparkleEffectManager.gameObject.SetActive(!pause);

            var effects = _sparkleEffectManager.GetField<
                ObstacleSaberSparkleEffect[], ObstacleSaberSparkleEffectManager>("_effects");

            foreach (var effect in effects)
                effect.gameObject.SetActive(!pause);
        }
        public void ModifyCombo(int combo, int maxCombo, bool isBroken = false)
        {
            _comboController.SetField("_combo", combo);
            _comboController.SetField("_maxCombo", maxCombo);
            _comboUIController.HandleComboDidChange(combo);
            _comboUIController.SetField("_fullComboLost", false);
            if (isBroken)
                _comboUIController.HandleComboBreakingEventHappened();
            else
                _comboUIController.GetField<Animator, ComboUIController>("_animator").Rebind();
        }
        public void ModifyDebrisPhysics(float multiplier)
        {
            _noteDebrisSpawner.SetField("_cutDirMultiplier", _debrisCutDirMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_moveSpeedMultiplier", _debrisMoveSpeedMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_fromCenterSpeed", _debrisFromCenterSpeed * multiplier);
        }

        #endregion

        #region Event Handlers

        private void HandlePauseStateChanged(bool state)
        {
            PauseSabersSparkles(state);
        }
        private void HandleSongSpeedChanged(float speedMul)
        {
            ModifyDebrisPhysics(speedMul);
        }
        private void HandleComboDidBreak()
        {
            _comboWasBroke = true;
        }
        private void HandleReprocessRequested()
        {
            _wasInProcess = _eventsProcessor.TimeWasSmallerThanActualTime;
            _comboWasBroke = false;
        }
        private void HandleReprocessDone()
        {
            if (_wasInProcess)
            {
                ModifyCombo(_comboController.GetField<int, ComboController>("_combo"), _comboController.maxCombo, _comboWasBroke);
                _wasInProcess = false;
            }
        }

        #endregion
    }
}
