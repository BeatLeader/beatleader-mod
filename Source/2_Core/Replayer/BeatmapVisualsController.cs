using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine.Playables;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
using HMUI;
using System.Linq;
using BeatLeader.Models.AbstractReplay;
using TMPro;

namespace BeatLeader.Replayer {
    public class BeatmapVisualsController : MonoBehaviour {
        #region Injection

        [Inject] private readonly IReplayPauseController _playbackController = null!;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly IReplayBeatmapEventsProcessor _beatmapEventsProcessor = null!;
        [Inject] private readonly ComboController _comboController = null!;
        [Inject] private readonly GameEnergyCounter _gameEnergyCounter = null!;

        [FirstResource]
        private readonly ComboUIController _comboUIController = null!;

        [FirstResource(requireActiveInHierarchy: true)]
        private readonly GameEnergyUIPanel _gameEnergyUIPanel = null!;

        [FirstResource(requireActiveInHierarchy: true)]
        private readonly ObstacleSaberSparkleEffectManager _sparkleEffectManager = null!;

        [FirstResource(requireActiveInHierarchy: true)]
        private readonly NoteDebrisSpawner _noteDebrisSpawner = null!;

        [FirstResource(requireActiveInHierarchy: true)]
        private readonly SaberBurnMarkSparkles _saberBurnMarkSparkles = null!;

        [FirstResource]
        private readonly LevelFailedTextEffect _levelFailedTextEffect = null!;

        #endregion

        #region Setup

        private TextMeshPro _levelFailedEffectText = null!;
        private GameObject _laser = null!;
        private ImageView _energyIconEmpty = null!;
        private ImageView _energyIconFull = null!;

        private float _debrisCutDirMultiplier;
        private float _debrisFromCenterSpeed;
        private float _debrisMoveSpeedMultiplier;
        private bool _comboWasBroke;
        private bool _wasInProcess;

        private void Awake() {
            this.LoadResources();

            _levelFailedEffectText = _levelFailedTextEffect.transform.Find("Text").GetComponent<TextMeshPro>();
            var images = _gameEnergyUIPanel.GetComponentsInChildren<ImageView>();
            _energyIconEmpty = images.First(x => x.name == "EnergyIconEmpty");
            _energyIconFull = images.First(x => x.name == "EnergyIconFull");
            _laser = _gameEnergyUIPanel.transform.Find("Laser").gameObject;

            _debrisCutDirMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_cutDirMultiplier");
            _debrisFromCenterSpeed = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_fromCenterSpeed");
            _debrisMoveSpeedMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_moveSpeedMultiplier");

            _playbackController.PauseStateChangedEvent += HandlePauseStateChanged;
            _beatmapTimeController.SongSpeedWasChangedEvent += HandleSongSpeedChanged;
            _comboController.comboBreakingEventHappenedEvent += HandleComboDidBreak;
            _beatmapEventsProcessor.NoteEventDequeuedEvent += HandleNoteBeatmapEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent += HandleQueueAdjustStartedStarted;
        }

        private void Start() {
            ModifyLevelFailedTextEffect(false);
        }

        private void OnDestroy() {
            _cutScoreSpawnerSilencer.Dispose();

            _playbackController.PauseStateChangedEvent -= HandlePauseStateChanged;
            _beatmapTimeController.SongSpeedWasChangedEvent -= HandleSongSpeedChanged;
            _comboController.comboBreakingEventHappenedEvent -= HandleComboDidBreak;
            _beatmapEventsProcessor.NoteEventDequeuedEvent -= HandleNoteBeatmapEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent -= HandleQueueAdjustStartedStarted;
        }

        #endregion

        #region Visuals Control

        private readonly HarmonySilencer _cutScoreSpawnerSilencer = new(
            typeof(NoteCutScoreSpawner).GetMethod(nameof(NoteCutScoreSpawner.HandleScoringForNoteStarted))!, false
        );

        public void PauseCutScoreSpawner(bool pause) {
            _cutScoreSpawnerSilencer.Enabled = pause;
        }

        public void PauseSabersSparkles(bool pause) {
            _saberBurnMarkSparkles.enabled = !pause;
            _sparkleEffectManager.gameObject.SetActive(!pause);

            var effects = _sparkleEffectManager.GetField<
                ObstacleSaberSparkleEffect[], ObstacleSaberSparkleEffectManager>("_effects");

            foreach (var effect in effects)
                effect.gameObject.SetActive(!pause);
        }

        public void ModifyLevelFailedTextEffect(bool showText) {
            _levelFailedEffectText.enabled = showText;
        }

        public void ModifyComboPanel(int combo, int maxCombo, bool shouldBeBroken = false) {
            _comboController.SetField("_combo", combo);
            _comboController.SetField("_maxCombo", maxCombo);
            _comboUIController.HandleComboDidChange(combo);
            _comboUIController.SetField("_fullComboLost", false);
            if (shouldBeBroken)
                _comboUIController.HandleComboBreakingEventHappened();
            else
                _comboUIController.GetField<Animator, ComboUIController>("_animator")?.Rebind();
        }

        public void ModifyEnergyPanel(float energy, bool shouldBeLost = false) {
            if (!_gameEnergyUIPanel.isActiveAndEnabled) return;

            var energyBar = _gameEnergyUIPanel.GetField<Image, GameEnergyUIPanel>("_energyBar");
            if (!shouldBeLost && !energyBar.enabled) {
                var director = _gameEnergyUIPanel.GetField<
                    PlayableDirector, GameEnergyUIPanel>("_playableDirector");
                director.Stop();
                director.RebindPlayableGraphOutputs();
                director.Evaluate();
                energyBar.enabled = true;
                _laser.SetActive(false);
                _energyIconFull.transform.localPosition = new(59, 0);
                _energyIconEmpty.transform.localPosition = new(-59, 0);
                _gameEnergyCounter.gameEnergyDidChangeEvent += _gameEnergyUIPanel.RefreshEnergyUI;
            }
            _gameEnergyUIPanel.RefreshEnergyUI(energy);
        }

        public void ModifyDebrisPhysics(float multiplier) {
            _noteDebrisSpawner.SetField("_cutDirMultiplier", _debrisCutDirMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_moveSpeedMultiplier", _debrisMoveSpeedMultiplier * multiplier);
            _noteDebrisSpawner.SetField("_fromCenterSpeed", _debrisFromCenterSpeed * multiplier);
        }

        #endregion

        #region Callbacks

        private void HandlePauseStateChanged(bool state) {
            PauseSabersSparkles(state);
        }

        private void HandleSongSpeedChanged(float speedMul) {
            ModifyDebrisPhysics(speedMul);
        }

        private void HandleComboDidBreak() {
            _comboWasBroke = true;
        }

        private void HandleQueueAdjustStartedStarted() {
            _wasInProcess = _beatmapEventsProcessor.CurrentEventHasTimeMismatch;
            _comboWasBroke = false;
            PauseCutScoreSpawner(true);
        }

        private void HandleQueueAdjustFinished() {
            if (_wasInProcess) {
                var combo = _comboController.GetField<int, ComboController>("_combo");
                ModifyComboPanel(combo, _comboController.maxCombo, _comboWasBroke);
                _wasInProcess = false;
            }

            var didReach0Energy = _gameEnergyCounter.GetField<bool, GameEnergyCounter>("_didReach0Energy");
            ModifyEnergyPanel(_gameEnergyCounter.energy, didReach0Energy);
            PauseCutScoreSpawner(false);
        }

        private void HandleNoteBeatmapEventDequeued(LinkedListNode<NoteEvent> noteEventNode) {
            if (noteEventNode.Next?.Value.eventTime > _beatmapTimeController.SongTime) {
                HandleQueueAdjustFinished();
            }
        }

        #endregion
    }
}