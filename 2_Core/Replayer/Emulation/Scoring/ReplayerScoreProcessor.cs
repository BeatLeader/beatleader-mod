using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayerScoreProcessor : MonoBehaviour {
        #region Injection

        [Inject] private readonly ScoreController _scoreController;
        [Inject] private readonly ComboController _comboController;
        [Inject] private readonly GameEnergyCounter _gameEnergyCounter;
        [Inject] private readonly ReplayEventsProcessor _eventsProcessor;
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly IReadonlyBeatmapData _beatmapData;

        #endregion

        #region Setup

        private static readonly Models.NoteCutInfo emptyNoteCutInfo = new();

        private void Awake() {
            var sortedList = CreateSortedNoteDatasList(_beatmapData.allBeatmapDataItems);
            _generatedBeatmapNoteData = new LinkedList<NoteData>(sortedList);

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
            if (!TryFindNoteData(noteEvent, out NoteData noteData)) return false;
            var noteCutInfo = Models.NoteCutInfo.Convert(noteEvent?.noteCutInfo ?? emptyNoteCutInfo, noteData);
            _noteControllerEmulator.Setup(noteData, noteCutInfo);
            return true;
        }

        #endregion

        #region NoteData

        private class BeatmapDataItemsComparer : IComparer<BeatmapDataItem> {
            public bool invert;

            public int Compare(BeatmapDataItem left, BeatmapDataItem right) {
                var calc = left.time > right.time ? 1 : left.time < right.time ? -1 : 0;
                return invert ? -calc : calc;
            }
        }

        private static readonly BeatmapDataItemsComparer _beatmapItemsComparer = new();
        private LinkedList<NoteData> _generatedBeatmapNoteData;
        private LinkedListNode<NoteData> _startNodeForLastProcessedTime;

        private bool TryFindNoteData(NoteEvent noteEvent, out NoteData noteData) {
            var startNode = _startNodeForLastProcessedTime ?? _generatedBeatmapNoteData.First;
            var prevNode = _startNodeForLastProcessedTime;
            for (var node = startNode; node != null; node = node.Next) {
                noteData = node.Value;
                if (Mathf.Abs(_startNodeForLastProcessedTime?.Value.time ?? 0 - noteData.time) < 0.01f) {
                    _startNodeForLastProcessedTime = node;
                }
                if (noteEvent.IsMatch(noteData)) {
                    return true;
                }
            }
            _startNodeForLastProcessedTime = prevNode;
            noteData = null;
            Plugin.Log.Error("[Replayer] NoteData not found for id " + noteEvent.noteID);
            return false;
        }

        private static List<NoteData> CreateSortedNoteDatasList(IEnumerable<BeatmapDataItem> items) {
            var list = new List<NoteData>();
            foreach (var item in items) {
                if (!item.TryDefine(out NoteData noteData) && item.TryDefine(out SliderData sliderData)) {
                    noteData = NoteData.CreateBurstSliderNoteData(
                        sliderData.time, sliderData.headLineIndex, sliderData.headLineLayer,
                        sliderData.headBeforeJumpLineLayer, sliderData.colorType, NoteCutDirection.Any, 1f);
                }
                if (noteData != null) {
                    list.Add(noteData);
                }
            }
            list.Sort(_beatmapItemsComparer);
            return list;
        }

        #endregion

        #region Simulation

        private readonly NoteControllerEmulator _noteControllerEmulator =
            new GameObject("NoteControllerEmulator").AddComponent<NoteControllerEmulator>();

        private static bool _lastCutIsGood;
        private static float _lastCutBeforeCutRating;
        private static float _lastCutAfterCutRating;

        private void SimulateNoteWasCut(NoteEvent noteEvent, bool isGoodCut) {
            if (!SetupEmulator(noteEvent)) return;
            if (isGoodCut) {
                _lastCutBeforeCutRating = noteEvent.noteCutInfo.beforeCutRating;
                _lastCutAfterCutRating = noteEvent.noteCutInfo.afterCutRating;
                _lastCutIsGood = true;
            }
            _scoringMultisilencer.Enabled = false;
            try {
                _scoreController.HandleNoteWasSpawned(_noteControllerEmulator);
                _scoreController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _scoreController.LateUpdate();

                _gameEnergyCounter.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
                _comboController.HandleNoteWasCut(_noteControllerEmulator, _noteControllerEmulator.CutInfo);
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
            } finally {
                _scoringMultisilencer.Enabled = true;
            }
        }

        private static void FinishSaberSwingRatingCounter(SaberSwingRatingCounter counter, float beforeCutRating, float afterCutRating) {
            if (counter == null) return;
            counter.SetField("_beforeCutRating", Mathf.Clamp01(beforeCutRating));
            counter.SetField("_afterCutRating", Mathf.Clamp01(afterCutRating));
            counter.Finish();
        }

        #endregion

        #region Events

        private void HandleNoteProcessRequested(NoteEvent noteEvent) {
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
        private void HandleWallProcessRequested(WallEvent wallEvent) {
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
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasCut),ReflectionUtils.DefaultFlags),
            typeof(ComboController).GetMethod(nameof(ComboController.HandleNoteWasMissed), ReflectionUtils.DefaultFlags),
            typeof(ComboController).GetMethod(nameof(ComboController.HandlePlayerHeadDidEnterObstacles), ReflectionUtils.DefaultFlags),
        };

        private static readonly HarmonyPatchDescriptor _finishSwingRatingCounterPatchDescriptor = new(
            typeof(GoodCutScoringElement).GetMethod(nameof(
                GoodCutScoringElement.Init), ReflectionUtils.DefaultFlags), postfix:
            typeof(ReplayerScoreProcessor).GetMethod(nameof(
                GoodCutScoringInitPostfix), BindingFlags.NonPublic | BindingFlags.Static));

        private static readonly HarmonyPatchDescriptor _noteWasCutEnergyCounterPatchDescriptor = new(
            typeof(GameEnergyCounter).GetMethod(nameof(
                GameEnergyCounter.HandleNoteWasCut), ReflectionUtils.DefaultFlags), postfix:
            typeof(ReplayerScoreProcessor).GetMethod(nameof(
                NoteWasProcessedPostfix), ReflectionUtils.StaticFlags));

        private static readonly HarmonyPatchDescriptor _noteWasMissedEnergyCounterPatchDescriptor = new(
            typeof(GameEnergyCounter).GetMethod(nameof(
               GameEnergyCounter.HandleNoteWasMissed), ReflectionUtils.DefaultFlags), postfix:
            typeof(ReplayerScoreProcessor).GetMethod(nameof(
                NoteWasProcessedPostfix), ReflectionUtils.StaticFlags));

        private readonly HarmonySilencer _cutScoreSpawnerSilencer = new(
            typeof(NoteCutScoreSpawner).GetMethod(nameof(
                NoteCutScoreSpawner.HandleScoringForNoteStarted)), false);

        private readonly HarmonyAutoPatch _finishSwingRatingCounterPatch = new(_finishSwingRatingCounterPatchDescriptor);
        private readonly HarmonyAutoPatch _noteWasCutEnergyCounterPatch = new(_noteWasCutEnergyCounterPatchDescriptor);
        private readonly HarmonyAutoPatch _noteWasMissedEnergyCounterPatch = new(_noteWasMissedEnergyCounterPatchDescriptor);
        private readonly HarmonyMultisilencer _scoringMultisilencer = new(silencedMethods);

        private static void GoodCutScoringInitPostfix(GoodCutScoringElement __instance) {
            if (!_lastCutIsGood) return;

            var buffer = (CutScoreBuffer)__instance.cutScoreBuffer;
            var swingCounter = buffer.GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");

            FinishSaberSwingRatingCounter(swingCounter, _lastCutBeforeCutRating, _lastCutAfterCutRating);
            _lastCutIsGood = false;
        }

        private static void NoteWasProcessedPostfix(GameEnergyCounter __instance) {
            __instance.LateUpdate();
        }

        #endregion
    }
}