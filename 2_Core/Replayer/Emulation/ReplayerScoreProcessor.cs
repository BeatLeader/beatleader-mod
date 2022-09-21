using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using BeatLeader.Models;
using BeatLeader.Utils;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class ReplayerScoreProcessor : MonoBehaviour
    {
        [Inject] private readonly ScoreController _scoreController;
        [Inject] private readonly ComboController _comboController;
        [Inject] private readonly GameEnergyCounter _gameEnergyCounter;
        [Inject] private readonly ReplayEventsProcessor _eventsProcessor;

        private readonly NoteControllerEmulator _noteControllerEmulator = new();
        private SaberSwingRatingCounter _lastSaberSwingCounter;
        private static bool _isReprocessingNow;

        protected virtual void Awake()
        {
            ApplyPatches();
            EnableSilencer();

            _eventsProcessor.OnNoteCutRequested += NotifyNoteCutRequested;
            _eventsProcessor.OnWallInteractionRequested += NotifyWallInteractionRequested;
            _eventsProcessor.OnReprocessRequested += NotifyReprocessRequested;

            _scoreController.scoringForNoteStartedEvent += NotifyScoringForNoteStarted;
            _scoreController.scoringForNoteFinishedEvent += NotifyScoringForNoteFinished;
        }
        protected virtual void OnDestroy()
        {
            RemovePatches();

            _eventsProcessor.OnNoteCutRequested -= NotifyNoteCutRequested;
            _eventsProcessor.OnWallInteractionRequested -= NotifyWallInteractionRequested;
            _eventsProcessor.OnReprocessRequested -= NotifyReprocessRequested;

            _scoreController.scoringForNoteStartedEvent -= NotifyScoringForNoteStarted;
            _scoreController.scoringForNoteFinishedEvent -= NotifyScoringForNoteFinished;   
        }

        private void SimulateNoteWasCut(NoteEvent noteEvent, bool isGoodCut)
        {
            _noteControllerEmulator.Setup(noteEvent);

            DisableSilencer();
            try
            {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                if (isGoodCut) FinishSaberSwingRatingCounter(_lastSaberSwingCounter, 
                    noteEvent.noteCutInfo.beforeCutRating, noteEvent.noteCutInfo.afterCutRating);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _comboController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
            }
            finally
            {
                EnableSilencer();
            }
        }
        private void SimulateNoteWasMissed(NoteEvent noteEvent)
        {
            _noteControllerEmulator.Setup(noteEvent);

            DisableSilencer();
            try
            {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasMissed(_noteControllerEmulator);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasMissed(_noteControllerEmulator);
                _comboController.HandleNoteWasMissed(_noteControllerEmulator);
            }
            finally
            {
                EnableSilencer();
            }
        }
        private void FinishSaberSwingRatingCounter(SaberSwingRatingCounter counter, float beforeCutRating, float afterCutRating)
        {
            if (counter == null) return;
            counter.SetField("_beforeCutRating", Mathf.Clamp01(beforeCutRating));
            counter.SetField("_afterCutRating", Mathf.Clamp01(afterCutRating));
            counter.Finish();
        }

        #region Events

        private void NotifyNoteCutRequested(NoteEvent noteEvent)
        {
            switch (noteEvent.eventType)
            {
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
        private void NotifyWallInteractionRequested(WallEvent wallEvent)
        {
            DisableSilencer();
            _scoreController.HandlePlayerHeadDidEnterObstacles();
            _comboController.HandlePlayerHeadDidEnterObstacles();
            EnableSilencer();
        }
        private void NotifyReprocessRequested()
        {
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

            _isReprocessingNow = true;
        }
        private void NotifyReprocessDone()
        {
            _isReprocessingNow = false;
        }
        private void NotifyScoringForNoteStarted(ScoringElement element)
        {
            var goodScoringElement = element as GoodCutScoringElement;
            if (goodScoringElement == null) return;

            var cutScoreBuffer = goodScoringElement
                .GetField<CutScoreBuffer, GoodCutScoringElement>("_cutScoreBuffer");

            _lastSaberSwingCounter = cutScoreBuffer
                .GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");
        }
        private void NotifyScoringForNoteFinished(ScoringElement element)
        {
            _lastSaberSwingCounter = null;
        }

        #endregion

        #region Harmony

        private static readonly IReadOnlyList<MethodInfo> silencedMethods = new[] {
            // <------ ScoreController -------------
            typeof(ScoreController).GetMethod(nameof(ScoreController.LateUpdate), ReflectionUtils.DefaultFlags),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandleNoteWasSpawned), ReflectionUtils.DefaultFlags),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandleNoteWasCut), ReflectionUtils.DefaultFlags),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandleNoteWasMissed), ReflectionUtils.DefaultFlags),
            typeof(ScoreController).GetMethod(nameof(ScoreController.HandlePlayerHeadDidEnterObstacles), ReflectionUtils.DefaultFlags),
            // <------ GameEnergyCounter -----------
            typeof(GameEnergyCounter).GetMethod(nameof(GameEnergyCounter.HandleNoteWasCut), ReflectionUtils.DefaultFlags),
            typeof(GameEnergyCounter).GetMethod(nameof(GameEnergyCounter.HandleNoteWasMissed), ReflectionUtils.DefaultFlags),
            // <------ ComboController -------------
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasCut),ReflectionUtils.DefaultFlags),
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasMissed), ReflectionUtils.DefaultFlags),
            typeof(ComboController).GetMethod(nameof(ComboController.HandlePlayerHeadDidEnterObstacles), ReflectionUtils.DefaultFlags),
        };

        private static readonly HarmonyPatchDescriptor NoteCutScoreSpawnerPatch = new(
            typeof(NoteCutScoreSpawner).GetMethod(nameof(NoteCutScoreSpawner.HandleScoringForNoteStarted), BindingFlags.Instance | BindingFlags.Public),
            typeof(ReplayerScoreProcessor).GetMethod(nameof(NoteCutScoreSpawnerPrefix), BindingFlags.Static | BindingFlags.NonPublic)
        );

        private static Harmony _harmony;

        private static void ApplyPatches()
        {
            if (_harmony != null) return;
            _harmony = new Harmony("BL-Replayer-Main");
            _harmony.Patch(NoteCutScoreSpawnerPatch);
            ApplySilencerPatches(_harmony);
        }
        private static void RemovePatches()
        {
            if (_harmony == null) return;
            _harmony.UnpatchSelf();
            _harmony = null;
        }

        private static bool NoteCutScoreSpawnerPrefix()
        {
            return !_isReprocessingNow;
        }

        #region Silencers

        private static bool _isSilenced;

        private static void ApplySilencerPatches(Harmony harmony)
        {
            var silencedMethodPrefix = typeof(ReplayerScoreProcessor).GetMethod(
                nameof(SilencedMethodPrefix), BindingFlags.Static | BindingFlags.NonPublic);

            foreach (var silencedMethod in silencedMethods)
            {
                harmony.Patch(silencedMethod, new HarmonyMethod(silencedMethodPrefix));
            }
        }
        private static void EnableSilencer()
        {
            _isSilenced = true;
        }
        private static void DisableSilencer()
        {
            _isSilenced = false;
        }

        private static bool SilencedMethodPrefix()
        {
            return !_isSilenced;
        }

        #endregion

        #endregion
    }
}