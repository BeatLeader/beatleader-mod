using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Models;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [UsedImplicitly]
    internal class ReplayController : IInitializable, IDisposable {
        #region Harmony

        #region Apply/Remove patches

        private static Harmony _harmony;

        private static void ApplyPatches() {
            if (_harmony != null) return;
            _harmony = new Harmony("BL-Replayer-Main");
            _harmony.Patch(SaberSwingRatingCounterInitPatch);
            _harmony.Patch(NoteCutScoreSpawnerPatch);
            ApplySilencerPatches(_harmony);
        }

        private static void RemovePatches() {
            if (_harmony == null) return;
            _harmony.UnpatchSelf();
            _harmony = null;
        }

        #endregion

        #region Silencer patches // Suppresses non-emulated scoring events

        private static readonly IReadOnlyList<MethodInfo> silencedMethods = new[] {
            // <------ ScoreController -------------
            typeof(ScoreController).GetMethod(nameof(ScoreController.LateUpdate), BindingFlags.Instance | BindingFlags.Public),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandleNoteWasSpawned), BindingFlags.Instance | BindingFlags.Public),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandleNoteWasCut), BindingFlags.Instance | BindingFlags.Public),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandleNoteWasMissed), BindingFlags.Instance | BindingFlags.Public),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandlePlayerHeadDidEnterObstacles), BindingFlags.Instance | BindingFlags.Public),
            // <------ GameEnergyCounter -----------
            typeof(GameEnergyCounter).GetMethod(nameof(GameEnergyCounter.HandleNoteWasCut), BindingFlags.Instance | BindingFlags.Public),
            typeof(GameEnergyCounter).GetMethod(nameof(GameEnergyCounter.HandleNoteWasMissed), BindingFlags.Instance | BindingFlags.Public),
            // <------ ComboController -------------
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasCut), BindingFlags.Instance | BindingFlags.Public),
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasMissed), BindingFlags.Instance | BindingFlags.Public),
            typeof(ComboController).GetMethod(nameof(ComboController.HandlePlayerHeadDidEnterObstacles), BindingFlags.Instance | BindingFlags.Public),
        };

        private static void ApplySilencerPatches(Harmony harmony) {
            var silencedMethodPrefix = typeof(ReplayController).GetMethod(
                nameof(SilencedMethodPrefix), BindingFlags.Static | BindingFlags.NonPublic
            );

            foreach (var silencedMethod in silencedMethods) {
                harmony.Patch(silencedMethod, new HarmonyMethod(silencedMethodPrefix));
            }
        }

        #endregion

        #region SaberSwingRatingCounter Patch // Post swing emulation

        private static HarmonyPatchDescriptor SaberSwingRatingCounterInitPatch => new(
            typeof(SaberSwingRatingCounter).GetMethod(nameof(SaberSwingRatingCounter.Init), BindingFlags.Instance | BindingFlags.Public),
            typeof(ReplayController).GetMethod(nameof(SaberSwingRatingCounterInitPostfix), BindingFlags.Static | BindingFlags.NonPublic)
        );

        #endregion

        #region NoteCutScoreSpawner // Disables CutScore spawner on rewind

        private static HarmonyPatchDescriptor NoteCutScoreSpawnerPatch => new(
            typeof(NoteCutScoreSpawner).GetMethod(nameof(NoteCutScoreSpawner.HandleScoringForNoteStarted), BindingFlags.Instance | BindingFlags.Public),
            typeof(ReplayController).GetMethod(nameof(NoteCutScoreSpawnerPrefix), BindingFlags.Static | BindingFlags.NonPublic)
        );

        private static bool NoteCutScoreSpawnerPrefix() {
            return !ReplayEventsEmitter.IsRewinding;
        }

        #endregion

        #endregion

        #region Silencer // Suppresses non-emulated scoring events

        private static bool _isSilenced;

        private static bool SilencedMethodPrefix() {
            return !_isSilenced;
        }

        private static void EnableSilencer() {
            _isSilenced = true;
        }

        private static void DisableSilencer() {
            _isSilenced = false;
        }

        #endregion

        #region SaberSwingRatingCounter // Post swing emulation

        private static SaberSwingRatingCounter _lastSaberSwingCounter;

        // ReSharper disable once InconsistentNaming
        private static void SaberSwingRatingCounterInitPostfix(SaberSwingRatingCounter __instance) {
            _lastSaberSwingCounter = __instance;
        }

        private static void FinishSaberSwingRatingCounter(float beforeCutRating, float afterCutRating) {
            if (_lastSaberSwingCounter == null) return;
            _lastSaberSwingCounter.SetField("_beforeCutRating", Mathf.Clamp01(beforeCutRating));
            _lastSaberSwingCounter.SetField("_afterCutRating", Mathf.Clamp01(afterCutRating));
            _lastSaberSwingCounter.Finish();
            _lastSaberSwingCounter = null;
        }

        #endregion

        #region Inject / Initialize / Dispose

        [Inject, UsedImplicitly]
        private ScoreController _scoreController;

        [Inject, UsedImplicitly]
        private ComboController _comboController;

        [Inject, UsedImplicitly]
        private GameEnergyCounter _gameEnergyCounter;

        [Inject, UsedImplicitly]
        private ReplayEventsEmitter _eventsEmitter;

        public void Initialize() {
            ApplyPatches();
            EnableSilencer();

            _eventsEmitter.NoteEventAction += OnNoteEvent;
            _eventsEmitter.WallEventAction += OnWallEvent;
            _eventsEmitter.FullResetAction += OnFullReset;
        }

        public void Dispose() {
            RemovePatches();

            _eventsEmitter.NoteEventAction -= OnNoteEvent;
            _eventsEmitter.WallEventAction -= OnWallEvent;
            _eventsEmitter.FullResetAction -= OnFullReset;
        }

        #endregion

        #region OnNoteEvent

        private readonly NoteControllerEmulator _noteControllerEmulator = new();

        private void OnNoteEvent(NoteEvent noteEvent) {
            switch (noteEvent.eventType) {
                case NoteEventType.good:
                    SimulateNoteWasCut(noteEvent, true);
                    break;
                case NoteEventType.bad:
                case NoteEventType.bomb:
                    SimulateNoteWasCut(noteEvent, false);
                    break;
                case NoteEventType.miss:
                    SimulateNoteWasMissed(noteEvent);
                    break;
            }
        }

        private void SimulateNoteWasCut(NoteEvent noteEvent, bool isGoodCut) {
            _noteControllerEmulator.Setup(noteEvent);

            DisableSilencer();
            try {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                if (isGoodCut) FinishSaberSwingRatingCounter(noteEvent.noteCutInfo.beforeCutRating, noteEvent.noteCutInfo.afterCutRating);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _comboController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
            } finally {
                EnableSilencer();
            }
        }

        private void SimulateNoteWasMissed(NoteEvent noteEvent) {
            _noteControllerEmulator.Setup(noteEvent);

            DisableSilencer();
            try {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasMissed(_noteControllerEmulator);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasMissed(_noteControllerEmulator);
                _comboController.HandleNoteWasMissed(_noteControllerEmulator);
            } finally {
                EnableSilencer();
            }
        }

        #endregion

        #region OnWallEvent

        private void OnWallEvent(WallEvent wallEvent) {
            DisableSilencer();
            _scoreController.HandlePlayerHeadDidEnterObstacles();
            _comboController.HandlePlayerHeadDidEnterObstacles();
            EnableSilencer();
        }

        #endregion

        #region OnFullReset

        private void OnFullReset() {
            _scoreController.SetField("_modifiedScore", 0);
            _scoreController.SetField("_multipliedScore", 0);
            _scoreController.SetField("_immediateMaxPossibleMultipliedScore", 0);
            _scoreController.SetField("_immediateMaxPossibleModifiedScore", 0);
            _scoreController.SetField("_prevMultiplierFromModifiers", 0f);
            _scoreController.GetField<ScoreMultiplierCounter, ScoreController>("_maxScoreMultiplierCounter").Reset();
            _scoreController.GetField<ScoreMultiplierCounter, ScoreController>("_scoreMultiplierCounter").Reset();
            _scoreController.GetField<List<float>, ScoreController>("_sortedNoteTimesWithoutScoringElements").Clear();
            _scoreController.GetField<List<ScoringElement>, ScoreController>("_sortedScoringElementsWithoutMultiplier").Clear();
            _scoreController.GetField<List<ScoringElement>, ScoreController>("_scoringElementsWithMultiplier").Clear();
            _scoreController.GetField<List<ScoringElement>, ScoreController>("_scoringElementsToRemove").Clear();

            _gameEnergyCounter.SetField("_batteryLives", 4);
            _gameEnergyCounter.SetField("_didReach0Energy", false);
            _gameEnergyCounter.SetField("_nextFrameEnergyChange", 0f);
            _gameEnergyCounter.SetProperty("energy", _gameEnergyCounter.energyType != GameplayModifiers.EnergyType.Battery ? 0.5f : 1f);
            _gameEnergyCounter.ProcessEnergyChange(0f);

            _comboController.SetField("_combo", 0);
            _comboController.SetField("_maxCombo", 0);
        }

        #endregion
    }
}