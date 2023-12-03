﻿using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine.Playables;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
using HMUI;
using System.Linq;
using TMPro;

namespace BeatLeader.Replayer {
    public class BeatmapVisualsController : MonoBehaviour {
        #region Injection

        [Inject] private readonly IReplayPauseController _playbackController = null!;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly ComboController _comboController = null!;
        [Inject] private readonly GameEnergyCounter _gameEnergyCounter = null!;
        [Inject] private readonly ReplayEventsProcessor _eventsProcessor = null!;

        [FirstResource(requireActiveInHierarchy: true)]
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

        protected float _debrisCutDirMultiplier;
        protected float _debrisFromCenterSpeed;
        protected float _debrisMoveSpeedMultiplier;
        private bool _comboWasBroke;
        private bool _wasInProcess;

        private void Awake() {
            this.LoadResources();

            _levelFailedEffectText = _levelFailedTextEffect.transform.Find("Text").GetComponent<TextMeshPro>();
            var images = _gameEnergyUIPanel.GetComponentsInChildren<ImageView>();
            _energyIconEmpty = images.FirstOrDefault(x => x.name == "EnergyIconEmpty");
            _energyIconFull = images.FirstOrDefault(x => x.name == "EnergyIconFull");
            _laser = _gameEnergyUIPanel.transform.Find("Laser").gameObject;

            _debrisCutDirMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_cutDirMultiplier");
            _debrisFromCenterSpeed = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_fromCenterSpeed");
            _debrisMoveSpeedMultiplier = _noteDebrisSpawner.GetField<float, NoteDebrisSpawner>("_moveSpeedMultiplier");

            _playbackController.PauseStateChangedEvent += HandlePauseStateChanged;
            _beatmapTimeController.SongSpeedWasChangedEvent += HandleSongSpeedChanged;
            _eventsProcessor.ReprocessRequestedEvent += HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent += HandleReprocessDone;
            _comboController.comboBreakingEventHappenedEvent += HandleComboDidBreak;
        }

        private void Start() {
            ModifyLevelFailedTextEffect(false);
        }

        private void OnDestroy() {
            _playbackController.PauseStateChangedEvent -= HandlePauseStateChanged;
            _beatmapTimeController.SongSpeedWasChangedEvent -= HandleSongSpeedChanged;
            _eventsProcessor.ReprocessRequestedEvent -= HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent -= HandleReprocessDone;
            _comboController.comboBreakingEventHappenedEvent -= HandleComboDidBreak;
        }

        #endregion

        #region Visuals Control
 
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
                _comboUIController.GetField<Animator, ComboUIController>("_animator").Rebind();
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

        private void HandleReprocessRequested() {
            _wasInProcess = _eventsProcessor.TimeWasSmallerThanActualTime;
            _comboWasBroke = false;
        }

        private void HandleReprocessDone() {
            if (_wasInProcess) {
                ModifyComboPanel(_comboController.GetField<int,
                    ComboController>("_combo"), _comboController.maxCombo, _comboWasBroke);
                _wasInProcess = false;
            }

            ModifyEnergyPanel(_gameEnergyCounter.energy, _gameEnergyCounter
                .GetField<bool, GameEnergyCounter>("_didReach0Energy"));
        }

        #endregion
    }
}
