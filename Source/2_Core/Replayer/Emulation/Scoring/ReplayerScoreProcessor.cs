using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayerScoreProcessor : MonoBehaviour {
        #region Injection

        [Inject] private readonly ScoreController _scoreController = null!;
        [Inject] private readonly ComboController _comboController = null!;
        [Inject] private readonly GameEnergyCounter _gameEnergyCounter = null!;
        [Inject] private readonly IReplayBeatmapEventsProcessor _beatmapEventsProcessor = null!;
        [Inject] private readonly IReplayBeatmapData _replayBeatmapData = null!;

        #endregion

        #region Setup

        private int _lastEventIndex;

        private void Awake() {
            _noteControllerEmulator = new GameObject("NoteControllerEmulator").AddComponent<NoteControllerEmulator>();

            _beatmapEventsProcessor.NoteEventDequeuedEvent += HandleNoteBeatmapEventDequeued;
            _beatmapEventsProcessor.WallEventDequeuedEvent += HandleWallBeatmapEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent += HandleQueueAdjustStarted;
        }

        private void OnDestroy() {
            _noteWasCutEnergyCounterPatch.Dispose();
            _noteWasMissedEnergyCounterPatch.Dispose();
            _finishSwingRatingCounterPatch.Dispose();
            _scoringMultisilencer.Dispose();

            _beatmapEventsProcessor.NoteEventDequeuedEvent -= HandleNoteBeatmapEventDequeued;
            _beatmapEventsProcessor.WallEventDequeuedEvent -= HandleWallBeatmapEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent -= HandleQueueAdjustStarted;
        }

        private bool SetupEmulator(NoteEvent noteEvent) {
            _lastEventIndex = _replayBeatmapData.FindNoteDataForEvent(noteEvent, _lastEventIndex, out var noteData);
            if (noteData is null) return false;
            var noteCutInfo = noteEvent.noteCutInfo.SaturateNoteCutInfo(noteData!);
            _noteControllerEmulator.Setup(noteData!, noteCutInfo);
            return true;
        }

        #endregion

        #region Simulation

        private NoteControllerEmulator _noteControllerEmulator = null!;

        private static bool _lastCutIsGood;
        private static float _lastCutBeforeCutRating;
        private static float _lastCutAfterCutRating;
        private static LinkedListNode<NoteEvent>? _lastNoteEvent;

        private void SimulateNoteWasCut(NoteEvent noteEvent, bool isGoodCut) {
            if (!SetupEmulator(noteEvent)) return;
            if (isGoodCut) {
                _lastCutBeforeCutRating = noteEvent.beforeCutRating;
                _lastCutAfterCutRating = noteEvent.afterCutRating;
                _lastCutIsGood = true;
            }
            _scoringMultisilencer.Enabled = false;
            try {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _comboController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
            } catch (Exception e) {
                Plugin.Log.Debug(e);
            } finally {
                _scoringMultisilencer.Enabled = true;
            }
        }

        private void SimulateNoteWasMissed(NoteEvent noteEvent) {
            if (!SetupEmulator(noteEvent)) return;
            _scoringMultisilencer.Enabled = false;
            try {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasMissed(_noteControllerEmulator);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasMissed(_noteControllerEmulator);
                _comboController.HandleNoteWasMissed(_noteControllerEmulator);
            } catch (Exception e) {
                Plugin.Log.Debug(e);
            } finally {
                _scoringMultisilencer.Enabled = true;
            }
        }

        private static void FinishSaberSwingRatingCounter(SaberSwingRatingCounter? counter, float beforeCutRating, float afterCutRating) {
            if (counter == null) return;
            counter.SetField("_beforeCutRating", Mathf.Clamp01(beforeCutRating));
            counter.SetField("_afterCutRating", Mathf.Clamp01(afterCutRating));
            counter.Finish();
        }

        #endregion

        #region Callbacks

        private void HandleNoteBeatmapEventDequeued(LinkedListNode<NoteEvent> noteEventNode) {
            _lastNoteEvent = noteEventNode;
            var noteEvent = noteEventNode.Value;
            switch (noteEvent.eventType) {
                case NoteEvent.NoteEventType.GoodCut:
                    SimulateNoteWasCut(noteEvent, true);
                    break;
                case NoteEvent.NoteEventType.BadCut:
                case NoteEvent.NoteEventType.BombCut:
                    SimulateNoteWasCut(noteEvent, false);
                    break;
                case NoteEvent.NoteEventType.Miss:
                    SimulateNoteWasMissed(noteEvent);
                    break;
            }
            if (!_beatmapEventsProcessor.QueueIsBeingAdjusted) return;
            CountersPlusInterop.HandleMissedCounterNoteWasCut(_noteControllerEmulator.CutInfo);
        }

        private void HandleWallBeatmapEventDequeued(LinkedListNode<WallEvent> wallEventNode) {
            _scoringMultisilencer.Enabled = false;
            _scoreController.HandlePlayerHeadDidEnterObstacles();
            _comboController.HandlePlayerHeadDidEnterObstacles();
            _scoringMultisilencer.Enabled = true;
        }

        private void HandleQueueAdjustStarted() {
            _lastEventIndex = 0;
            if (!_beatmapEventsProcessor.CurrentEventHasTimeMismatch) return;

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

            CountersPlusInterop.ResetMissedCounter();
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
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasCut), ReflectionUtils.DefaultFlags),
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasMissed), ReflectionUtils.DefaultFlags),
            typeof(ComboController).GetMethod(nameof(ComboController.HandlePlayerHeadDidEnterObstacles), ReflectionUtils.DefaultFlags),
        };

        private static readonly HarmonyPatchDescriptor finishSwingRatingCounterPatchDescriptor = new(
            typeof(GoodCutScoringElement).GetMethod(
                nameof(GoodCutScoringElement.Init),
                ReflectionUtils.DefaultFlags
            )!,
            postfix:
            typeof(ReplayerScoreProcessor).GetMethod(
                nameof(GoodCutScoringInitPostfix),
                BindingFlags.NonPublic | BindingFlags.Static
            )
        );

        private static readonly HarmonyPatchDescriptor noteWasCutEnergyCounterPatchDescriptor = new(
            typeof(GameEnergyCounter).GetMethod(
                nameof(GameEnergyCounter.HandleNoteWasCut),
                ReflectionUtils.DefaultFlags
            )!,
            postfix:
            typeof(ReplayerScoreProcessor).GetMethod(
                nameof(NoteWasProcessedPostfix),
                ReflectionUtils.StaticFlags
            )
        );

        private static readonly HarmonyPatchDescriptor noteWasMissedEnergyCounterPatchDescriptor = new(
            typeof(GameEnergyCounter).GetMethod(
                nameof(GameEnergyCounter.HandleNoteWasMissed),
                ReflectionUtils.DefaultFlags
            )!,
            postfix:
            typeof(ReplayerScoreProcessor).GetMethod(
                nameof(NoteWasProcessedPostfix),
                ReflectionUtils.StaticFlags
            )
        );

        private readonly HarmonyAutoPatch _finishSwingRatingCounterPatch = new(finishSwingRatingCounterPatchDescriptor);
        private readonly HarmonyAutoPatch _noteWasCutEnergyCounterPatch = new(noteWasCutEnergyCounterPatchDescriptor);
        private readonly HarmonyAutoPatch _noteWasMissedEnergyCounterPatch = new(noteWasMissedEnergyCounterPatchDescriptor);
        private readonly HarmonyMultisilencer _scoringMultisilencer = new(silencedMethods);

        private static void GoodCutScoringInitPostfix(GoodCutScoringElement __instance) {
            if (!_lastCutIsGood) return;

            var buffer = (CutScoreBuffer)__instance.cutScoreBuffer;
            var swingCounter = buffer.GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");

            FinishSaberSwingRatingCounter(swingCounter, _lastCutBeforeCutRating, _lastCutAfterCutRating);
            _lastCutIsGood = false;
        }

        private static void NoteWasProcessedPostfix(GameEnergyCounter __instance) {
            if (_lastNoteEvent == null) return;
            var currentTime = _lastNoteEvent.Value.eventTime;
            var prevTime = _lastNoteEvent.Previous?.Value.eventTime ?? 0;
            var nextTime = _lastNoteEvent.Next?.Value.eventTime ?? 0;
            //if does not have more notes on this frame updating the energy
            if (Mathf.Abs(currentTime - nextTime) > 1e-6 && Mathf.Abs(currentTime - prevTime) > 1e-6) __instance.LateUpdate();
        }

        #endregion
    }
}