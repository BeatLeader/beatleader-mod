using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        [Inject] private readonly ReplayEventsProcessor _eventsProcessor = null!;
        [Inject] private readonly IReadonlyBeatmapData _beatmapData = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        #endregion

        #region Setup

        private void Awake() {
            var sortedList = CreateSortedNoteDataList(_beatmapData.allBeatmapDataItems);
            _generatedBeatmapNoteData = new LinkedList<NoteData>(sortedList);
            _noteControllerEmulator = new GameObject("NoteControllerEmulator").AddComponent<NoteControllerEmulator>();
            _comparator = _launchData.ReplayComparator;
            _eventsProcessor.NoteProcessRequestedEvent += HandleNoteProcessRequested;
            _eventsProcessor.WallProcessRequestedEvent += HandleWallProcessRequested;
            _eventsProcessor.ReprocessRequestedEvent += HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent += HandleReprocessDone;
        }

        private void OnDestroy() {
            _eventsProcessor.NoteProcessRequestedEvent -= HandleNoteProcessRequested;
            _eventsProcessor.WallProcessRequestedEvent -= HandleWallProcessRequested;
            _eventsProcessor.ReprocessRequestedEvent -= HandleReprocessRequested;
            _eventsProcessor.ReprocessDoneEvent -= HandleReprocessDone;

            _noteWasCutEnergyCounterPatch.Dispose();
            _noteWasMissedEnergyCounterPatch.Dispose();
            _finishSwingRatingCounterPatch.Dispose();
            _scoringMultisilencer.Dispose();
            _cutScoreSpawnerSilencer.Dispose();
        }

        private bool SetupEmulator(NoteEvent noteEvent) {
            if (!TryFindNoteData(noteEvent, out var noteData)) return false;
            var noteCutInfo = noteEvent.noteCutInfo.SaturateNoteCutInfo(noteData!);
            _noteControllerEmulator.Setup(noteData!, noteCutInfo);
            return true;
        }

        #endregion

        #region NoteData

        private class BeatmapDataItemsComparer : IComparer<BeatmapDataItem> {
            public int Compare(BeatmapDataItem left, BeatmapDataItem right) {
                return left.time > right.time ? 1 : left.time < right.time ? -1 : 0;
            }
        }

        private static readonly BeatmapDataItemsComparer beatmapItemsComparer = new();
        private LinkedList<NoteData> _generatedBeatmapNoteData = null!;
        private LinkedListNode<NoteData>? _startNodeForLastProcessedTime;
        private IReplayComparator _comparator = null!;

        private bool TryFindNoteData(NoteEvent noteEvent, out NoteData? noteData) {
            var startNode = _startNodeForLastProcessedTime ?? _generatedBeatmapNoteData.First;
            var prevNode = _startNodeForLastProcessedTime;
            for (var node = startNode; node != null; node = node.Next) {
                noteData = node.Value;
                if (Mathf.Abs(_startNodeForLastProcessedTime?.Value.time ?? 0 - noteData.time) < 1e-3) {
                    _startNodeForLastProcessedTime = node;
                }
                if (Mathf.Abs(noteEvent.spawnTime - noteData.time) < 1e-3
                    && _comparator.Compare(noteEvent, noteData)) {
                    return true;
                }
            }
            _startNodeForLastProcessedTime = prevNode;
            noteData = null;
            Plugin.Log.Error("[Replayer] Not found NoteData for id " + noteEvent.noteId);
            return false;
        }

        private static IEnumerable<NoteData> CreateSortedNoteDataList(IEnumerable<BeatmapDataItem> items) {
            var result = new List<NoteData>();

            foreach (var item in items) {
                var data = item as NoteData;
                if (data != null) {
                    result.Add(data);
                } else {
                    var sliderData = item as SliderData;
                    if (sliderData != null) {
                        int sliceCount = sliderData.sliceCount;
                        for (int i = 1; i < sliceCount; ++i) {
                            int lineIndex = i < sliceCount - 1 ? sliderData.headLineIndex : sliderData.tailLineIndex;
                            NoteLineLayer noteLineLayer = i < sliceCount - 1 ? sliderData.headLineLayer : sliderData.tailLineLayer;
                            result.Add(NoteData.CreateBurstSliderNoteData(
                                Mathf.LerpUnclamped(sliderData.time, sliderData.tailTime, (float)i / (float)(sliceCount - 1)), lineIndex, noteLineLayer,
                                sliderData.headBeforeJumpLineLayer, sliderData.colorType, NoteCutDirection.Any, 1f));
                        }
                    }
                }
            }

            return result.OrderBy(static x => x, beatmapItemsComparer);
        }

        #endregion

        #region Simulation

        private NoteControllerEmulator _noteControllerEmulator = null!;

        private static bool _lastCutIsGood;
        private static float _lastCutBeforeCutRating;
        private static float _lastCutAfterCutRating;
        private static LinkedListNode<NoteEvent> _lastNoteEvent = null!;

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

        private void HandleNoteProcessRequested(LinkedListNode<NoteEvent> noteEventNode) {
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
            if (!_eventsProcessor.IsReprocessingEventsNow) return;
            CountersPlusInterop.HandleMissedCounterNoteWasCut(_noteControllerEmulator.CutInfo);
        }

        private void HandleWallProcessRequested(LinkedListNode<WallEvent> wallEventNode) {
            _scoringMultisilencer.Enabled = false;
            _scoreController.HandlePlayerHeadDidEnterObstacles();
            _comboController.HandlePlayerHeadDidEnterObstacles();
            _scoringMultisilencer.Enabled = true;
        }

        private void HandleReprocessRequested() {
            _cutScoreSpawnerSilencer.Enabled = true;
            _startNodeForLastProcessedTime = null;
            if (!_eventsProcessor.TimeWasSmallerThanActualTime) return;

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

        private void HandleReprocessDone() {
            _cutScoreSpawnerSilencer.Enabled = false;
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
            typeof(GoodCutScoringElement).GetMethod(nameof(
                GoodCutScoringElement.Init), ReflectionUtils.DefaultFlags)!, postfix:
            typeof(ReplayerScoreProcessor).GetMethod(nameof(
                GoodCutScoringInitPostfix), BindingFlags.NonPublic | BindingFlags.Static));

        private static readonly HarmonyPatchDescriptor noteWasCutEnergyCounterPatchDescriptor = new(
            typeof(GameEnergyCounter).GetMethod(nameof(
                GameEnergyCounter.HandleNoteWasCut), ReflectionUtils.DefaultFlags)!, postfix:
            typeof(ReplayerScoreProcessor).GetMethod(nameof(
                NoteWasProcessedPostfix), ReflectionUtils.StaticFlags));

        private static readonly HarmonyPatchDescriptor noteWasMissedEnergyCounterPatchDescriptor = new(
            typeof(GameEnergyCounter).GetMethod(nameof(
                GameEnergyCounter.HandleNoteWasMissed), ReflectionUtils.DefaultFlags)!, postfix:
            typeof(ReplayerScoreProcessor).GetMethod(nameof(
                NoteWasProcessedPostfix), ReflectionUtils.StaticFlags));

        private readonly HarmonySilencer _cutScoreSpawnerSilencer = new(
            typeof(NoteCutScoreSpawner).GetMethod(nameof(NoteCutScoreSpawner
                .HandleScoringForNoteStarted), ReflectionUtils.DefaultFlags)!, false);

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
            var currentTime = _lastNoteEvent.Value.eventTime;
            var prevTime = _lastNoteEvent.Previous?.Value.eventTime ?? 0;
            var nextTime = _lastNoteEvent.Next?.Value.eventTime ?? 0;
            //if does not have more notes on this frame updating the energy
            if (Mathf.Abs(currentTime - nextTime) > 1e-6 && Mathf.Abs(currentTime - prevTime) > 1e-6) __instance.LateUpdate();
        }

        #endregion
    }
}