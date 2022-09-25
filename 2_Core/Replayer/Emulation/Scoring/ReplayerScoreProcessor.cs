using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class ReplayerScoreProcessor : MonoBehaviour
    {
        [Inject] private readonly ScoreController _scoreController;
        [Inject] private readonly ComboController _comboController;
        [Inject] private readonly GameEnergyCounter _gameEnergyCounter;
        [Inject] private readonly ReplayEventsProcessor _eventsProcessor;

        private readonly NoteControllerEmulator _noteControllerEmulator = new();
        private SaberSwingRatingCounter _lastSaberSwingCounter;

        private void Awake()
        {
            _eventsProcessor.NoteCutRequestedEvent += HandleNoteCutRequested;
            _eventsProcessor.WallInteractionRequestedEvent += HandleWallInteractionRequested;
            _eventsProcessor.ReprocessRequestedEvent += HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent += HandleReprocessDone;

            _scoreController.scoringForNoteStartedEvent += HandleScoringForNoteStarted;
        }
        private void OnDestroy()
        {
            _eventsProcessor.NoteCutRequestedEvent -= HandleNoteCutRequested;
            _eventsProcessor.WallInteractionRequestedEvent -= HandleWallInteractionRequested;
            _eventsProcessor.ReprocessRequestedEvent -= HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent -= HandleReprocessDone;

            _scoreController.scoringForNoteStartedEvent -= HandleScoringForNoteStarted;

            _scoringMultisilencer.Dispose();
            _cutScoreSpawnerSilencer.Dispose();
        }

        protected void SimulateNoteWasCut(NoteEvent noteEvent, bool isGoodCut)
        {
            _noteControllerEmulator.Setup(noteEvent);

            _scoringMultisilencer.Enabled = false;
            try
            {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                if (isGoodCut) FinishSaberSwingRatingCounter(
                        noteEvent.noteCutInfo.beforeCutRating, noteEvent.noteCutInfo.afterCutRating);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _comboController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
            }
            finally
            {
                _scoringMultisilencer.Enabled = true;
            }
        }
        protected void SimulateNoteWasMissed(NoteEvent noteEvent)
        {
            _noteControllerEmulator.Setup(noteEvent);

            _scoringMultisilencer.Enabled = false;
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
                _scoringMultisilencer.Enabled = true;
            }
        }
        private void FinishSaberSwingRatingCounter(float beforeCutRating, float afterCutRating)
        {
            if (_lastSaberSwingCounter == null) return;
            _lastSaberSwingCounter.SetField("_beforeCutRating", Mathf.Clamp01(beforeCutRating));
            _lastSaberSwingCounter.SetField("_afterCutRating", Mathf.Clamp01(afterCutRating));
            _lastSaberSwingCounter.Finish();
        }

        #region Events

        private void HandleNoteCutRequested(NoteEvent noteEvent)
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
        private void HandleWallInteractionRequested(WallEvent wallEvent)
        {
            _scoringMultisilencer.Enabled = false;
            _scoreController.HandlePlayerHeadDidEnterObstacles();
            _comboController.HandlePlayerHeadDidEnterObstacles();
            _scoringMultisilencer.Enabled = true;
        }
        private void HandleReprocessRequested()
        {
            _cutScoreSpawnerSilencer.Enabled = true;
            if (!_eventsProcessor.IsReprocessingEventsNow) return;

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
        private void HandleReprocessDone()
        {
            _cutScoreSpawnerSilencer.Enabled = false;
        }
        private void HandleScoringForNoteStarted(ScoringElement element)
        {
            var goodScoringElement = element as GoodCutScoringElement;
            if (goodScoringElement == null) return;

            var cutScoreBuffer = goodScoringElement
                .GetField<CutScoreBuffer, GoodCutScoringElement>("_cutScoreBuffer");

            _lastSaberSwingCounter = cutScoreBuffer
                .GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");
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

        private HarmonyMultisilencer _scoringMultisilencer = new(silencedMethods);
        private HarmonySilencer _cutScoreSpawnerSilencer = new(typeof(NoteCutScoreSpawner)
            .GetMethod(nameof(NoteCutScoreSpawner.HandleScoringForNoteStarted)), false);

        #endregion
    }
}