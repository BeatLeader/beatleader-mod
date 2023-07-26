using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Core.Managers.NoteEnhancer;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using HarmonyLib;
using IPA.Loader;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Transform = BeatLeader.Models.Replay.Transform;

namespace BeatLeader {
    [UsedImplicitly]
    public class ReplayRecorder : IInitializable, IDisposable, ILateTickable {
        #region Harmony

        #region Patching

        private static ReplayRecorder _instance;
        private Harmony _harmony;

        private void InitializePatches() {
            _instance = this;
            _harmony ??= new Harmony("BeatLeader.ReplayRecorder");
            _harmony.Patch(LateUpdatePatchDescriptor);
        }

        private void DisposePatches() {
            _harmony.UnpatchSelf();
            _instance = null;
        }

        #endregion

        #region ScoreController.LateUpdate Patch

        private static HarmonyPatchDescriptor LateUpdatePatchDescriptor => new(
            typeof(ScoreController).GetMethod(nameof(ScoreController.LateUpdate), BindingFlags.Instance | BindingFlags.Public),
            typeof(ReplayRecorder).GetMethod(nameof(ScoreControllerLateUpdatePrefix), BindingFlags.Static | BindingFlags.NonPublic)
        );

        // ReSharper disable InconsistentNaming
        private static void ScoreControllerLateUpdatePrefix(
            AudioTimeSyncController ____audioTimeSyncController,
            List<float> ____sortedNoteTimesWithoutScoringElements,
            List<ScoringElement> ____sortedScoringElementsWithoutMultiplier
        ) {
            _instance.OnBeforeScoreControllerLateUpdate(
                ____audioTimeSyncController,
                ____sortedNoteTimesWithoutScoringElements,
                ____sortedScoringElementsWithoutMultiplier
            );
        }

        #endregion

        #endregion
        
        [Inject] [UsedImplicitly] private PlayerTransforms _playerTransforms;
        [Inject] [UsedImplicitly] private BeatmapObjectManager _beatmapObjectManager;
        [Inject] [UsedImplicitly] private BeatmapObjectSpawnController _beatSpawnController;
        [Inject] [UsedImplicitly] private StandardLevelScenesTransitionSetupDataSO _transitionSetup;
        [Inject] [UsedImplicitly] private MultiplayerLevelScenesTransitionSetupDataSO _mpTransitionSetup;
        [Inject] [UsedImplicitly] private AudioTimeSyncController _timeSyncController;
        [Inject] [UsedImplicitly] private ScoreController _scoreController;
        [Inject] [UsedImplicitly] private PlayerHeadAndObstacleInteraction _phaoi;
        [Inject] [UsedImplicitly] private GameEnergyCounter _gameEnergyCounter;
        [Inject] [UsedImplicitly] private TrackingDeviceEnhancer _trackingDeviceEnhancer;
        [InjectOptional] [UsedImplicitly] private PlayerHeightDetector _playerHeightDetector;
        // Optional for MP support, there is no pause mechanic in multiplayer gameplay.
        [InjectOptional][UsedImplicitly] private PauseController _pauseController;

        private readonly Replay _replay = new();
        private Pause _currentPause;
        private WallEvent _currentWallEvent;
        private DateTime _pauseStartTime;
        private bool _stopRecording;

        public ReplayRecorder() {
            UserEnhancer.Enhance(_replay); 

            PluginMetadata metaData = PluginManager.GetPluginFromId("BeatLeader");
            _replay.info.version = metaData.HVersion.ToString();
            _replay.info.gameVersion = Application.version;
            _replay.info.timestamp = Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            _stopRecording = false;
        }

        private readonly Dictionary<int, NoteEvent> _noteEventCache = new();

        private readonly Dictionary<NoteData, int> _noteIdCache = new();
        private int _noteId;

        private readonly Dictionary<ObstacleController, int> _wallCache = new();
        private readonly Dictionary<int, WallEvent> _wallEventCache = new();
        private int _wallId;

        public void Initialize() {
            InitializePatches();
            
            _beatmapObjectManager.noteWasAddedEvent += OnNoteWasAdded;
            _beatmapObjectManager.obstacleWasSpawnedEvent += OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasCutEvent += OnNoteWasCut;
            _transitionSetup.didFinishEvent += OnTransitionSetupOnDidFinishEvent;
            _mpTransitionSetup.didFinishEvent += OnMultiplayerTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent += OnBeatSpawnControllerDidInit;
            _phaoi.headDidEnterObstacleEvent += OnObstacle;
            _scoreController.scoringForNoteFinishedEvent += OnScoringDidFinish;
            if (_pauseController != null) {
                _pauseController.didPauseEvent += OnPause;
                _pauseController.didResumeEvent += OnResume;
            }

            if (_playerHeightDetector != null) {
                _playerHeightDetector.playerHeightDidChangeEvent += OnPlayerHeightChange;
            }

            ScoreUtil.EnableSubmission();
        }

        public void Dispose() {
            DisposePatches();
            
            _beatmapObjectManager.noteWasAddedEvent -= OnNoteWasAdded;
            _beatmapObjectManager.obstacleWasSpawnedEvent -= OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasCutEvent -= OnNoteWasCut;
            _transitionSetup.didFinishEvent -= OnTransitionSetupOnDidFinishEvent;
            _mpTransitionSetup.didFinishEvent -= OnMultiplayerTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent -= OnBeatSpawnControllerDidInit;
            _phaoi.headDidEnterObstacleEvent -= OnObstacle;
            _scoreController.scoringForNoteFinishedEvent -= OnScoringDidFinish;
            if (_pauseController != null) {
                _pauseController.didPauseEvent -= OnPause;
                _pauseController.didResumeEvent -= OnResume;
            }

            if (_playerHeightDetector != null)
            {
                _playerHeightDetector.playerHeightDidChangeEvent -= OnPlayerHeightChange;
            }

        }

        public void LateTick() {
            if (_timeSyncController == null || _playerTransforms == null || _currentPause != null || _stopRecording) return;

            var frame = new Frame() {
                time = _timeSyncController.songTime,
                fps = Mathf.RoundToInt(1.0f / Time.deltaTime),
                head = new Transform {
                    rotation = _playerTransforms.headPseudoLocalRot,
                    position = _playerTransforms.headPseudoLocalPos
                },
                leftHand = new Transform {
                    rotation = _playerTransforms.leftHandPseudoLocalRot,
                    position = _playerTransforms.leftHandPseudoLocalPos
                },
                rightHand = new Transform {
                    rotation = _playerTransforms.rightHandPseudoLocalRot,
                    position = _playerTransforms.rightHandPseudoLocalPos
                }
            };

            _replay.frames.Add(frame);

            if (_currentWallEvent != null) {
                if (_phaoi != null && !_phaoi.playerHeadIsInObstacle)
                {
                    _currentWallEvent.energy = _gameEnergyCounter.energy;
                    _currentWallEvent = null;
                }
            }
        }

        private void OnNoteWasAdded(NoteData noteData, BeatmapObjectSpawnMovementData.NoteSpawnData spawnData, float rotation) {
            if (_stopRecording) { return; }

            var noteId = _noteId++;
            _noteIdCache[noteData] = noteId;
            NoteEvent noteEvent = new();
            noteEvent.noteID = ((int)noteData.scoringType + 2) * 10000 + noteData.lineIndex * 1000 + (int)noteData.noteLineLayer * 100 + (int)noteData.colorType * 10 + (int)noteData.cutDirection;
            noteEvent.spawnTime = noteData.time;
            _noteEventCache[noteId] = noteEvent;
        }

        private void OnObstacleWasSpawned(ObstacleController obstacleController) {
            if (_stopRecording) { return; }

            var wallId = _wallId++;
            _wallCache[obstacleController] = wallId;

            var obstacleData = obstacleController.obstacleData;

            WallEvent wallEvent = new();
            wallEvent.wallID = obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
            wallEvent.spawnTime = obstacleData.time;
            _wallEventCache[wallId] = wallEvent;
        }

        private void OnObstacle(ObstacleController obstacle) {
            if (_stopRecording) { return; }

            if (_currentWallEvent == null)
            {
                WallEvent wallEvent = _wallEventCache[_wallCache[obstacle]];
                wallEvent.time = _timeSyncController.songTime;
                _replay.walls.Add(wallEvent);
                _currentWallEvent = wallEvent;
            }
        }

        private void OnNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo) {
            if (_stopRecording) { return; }

            var noteId = _noteIdCache[noteCutInfo.noteData];
            var noteEvent = _noteEventCache[noteId];
            noteEvent.noteCutInfo = noteCutInfo;

            if (UnityEngine.Vector3.Dot(noteCutInfo.cutNormal, noteCutInfo.cutPoint - noteController.noteTransform.position) <= 0) {
                noteEvent.noteCutInfo.cutDistanceToCenterPositive = true;
            }
        }

        private void OnBeforeScoreControllerLateUpdate(
            AudioTimeSyncController audioTimeSyncController,
            List<float> sortedNoteTimesWithoutScoringElements,
            List<ScoringElement> sortedScoringElementsWithoutMultiplier
        ) {
            if (_stopRecording) { return; }

            var songTime = audioTimeSyncController.songTime;
            var nearestNotCutNoteTime = sortedNoteTimesWithoutScoringElements.Count > 0 ? sortedNoteTimesWithoutScoringElements[0] : float.MaxValue;
            var skipAfter = songTime + 0.15f;

            foreach (var scoringElement in sortedScoringElementsWithoutMultiplier) {
                if (scoringElement.time >= skipAfter && scoringElement.time <= nearestNotCutNoteTime) break;
                
                var noteData = scoringElement.noteData;
                if (scoringElement is MissScoringElement && noteData.scoringType == NoteData.ScoringType.NoScore) continue;

                var noteId = _noteIdCache[noteData];
                var noteEvent = _noteEventCache[noteId];
                noteEvent.eventTime = songTime;
                _replay.notes.Add(noteEvent);
            }
        }

        private void OnScoringDidFinish(ScoringElement scoringElement) {
            if (_stopRecording) { return; }

            var noteData = scoringElement.noteData;
            var noteId = _noteIdCache[noteData];
            var noteEvent = _noteEventCache[noteId];
            var isBomb = noteData.colorType == ColorType.None;

            switch (scoringElement) {
                case MissScoringElement: {
                    if (isBomb) return;
                    noteEvent.eventType = NoteEventType.miss;
                    break;
                }
                case BadCutScoringElement: {
                    noteEvent.eventType = isBomb ? NoteEventType.bomb : NoteEventType.bad;
                    break;
                }
                case GoodCutScoringElement goodCut: {
                    var cutScoreBuffer = goodCut.GetField<CutScoreBuffer, GoodCutScoringElement>("_cutScoreBuffer");
                    var saberSwingRatingCounter = cutScoreBuffer.GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");
                    SwingRatingEnhancer.Enhance(noteEvent.noteCutInfo, saberSwingRatingCounter);
                    SwingRatingEnhancer.Reset(saberSwingRatingCounter);
                    noteEvent.eventType = NoteEventType.good;
                    break;
                }
            }
        }

        private void OnBeatSpawnControllerDidInit()
        {
            _replay.info.jumpDistance = _beatSpawnController.jumpDistance;
        }

        private void OnTransitionSetupOnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results)
        {
            _stopRecording = true;
            _replay.notes.RemoveAll(note => note.eventType == NoteEventType.unknown);

            _replay.info.score = results.multipliedScore;
            MapEnhancer.energy = results.energy; 
            MapEnhancer.Enhance(_replay);
            _trackingDeviceEnhancer.Enhance(_replay);

            PlayEndData playEndData = new(results);
            if (playEndData.EndType == LevelEndType.Fail) {
                _replay.info.failTime = _timeSyncController.songTime;
            }

            Plugin.Log.Debug($"Level result: {playEndData.EndType}, end time: {playEndData.Time}");
            if (_replay.notes.Count > 0) {
                ScoreUtil.ProcessReplay(_replay, playEndData);
            } else {
                Plugin.Log.Debug("Not enough notes to submit");
            }
        }

        private void OnMultiplayerTransitionSetupOnDidFinishEvent(MultiplayerLevelScenesTransitionSetupDataSO data, MultiplayerResultsData results)
        {
            _stopRecording = true;

            if (_replay != null && results != null && results.localPlayerResultData != null && results.localPlayerResultData.multiplayerLevelCompletionResults != null) {
                _replay.notes.RemoveAll(note => note.eventType == NoteEventType.unknown);

                var mpResults = results.localPlayerResultData.multiplayerLevelCompletionResults;
                var levelCompResults = mpResults.levelCompletionResults;
                _replay.info.score = levelCompResults.multipliedScore;
                MapEnhancer.energy = levelCompResults.energy;
                MapEnhancer.Enhance(_replay);
                _trackingDeviceEnhancer.Enhance(_replay);

                if (mpResults.playerLevelEndState == MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndState.SongFinished) {

                    PlayEndData playEndData = new(levelCompResults);
                    if (playEndData.EndType == LevelEndType.Fail) {
                        _replay.info.failTime = _timeSyncController.songTime;
                    }

                    Plugin.Log.Debug($"Level result: {playEndData.EndType}, end time: {playEndData.Time}");
                    if (_replay.notes.Count > 0) {
                        ScoreUtil.ProcessReplay(_replay, playEndData);
                    } else {
                        Plugin.Log.Debug("Not enough notes to submit");
                    }
                }
            }
        }

        private void OnPlayerHeightChange(float height) {
            if (_stopRecording) { return; }

            AutomaticHeight automaticHeight = new();
            automaticHeight.height = height;
            automaticHeight.time = _timeSyncController.songTime;

            _replay.heights.Add(automaticHeight);
        }

        private void OnPause() {
            if (_stopRecording) { return; }

            _currentPause = new();
            _currentPause.time = _timeSyncController.songTime;
            _pauseStartTime = DateTime.Now;
        }

        private void OnResume() {
            if (_stopRecording) { return; }

            _currentPause.duration = DateTime.Now.ToUnixTime() - _pauseStartTime.ToUnixTime();
            _replay.pauses.Add(_currentPause);
            _currentPause = null;
        }

        private static Models.Replay.NoteCutInfo CreateNoteCutInfo(NoteCutInfo cutInfo) {
            return new Models.Replay.NoteCutInfo {
                speedOK = cutInfo.speedOK,
                directionOK = cutInfo.directionOK,
                saberTypeOK = cutInfo.saberTypeOK,
                wasCutTooSoon = cutInfo.wasCutTooSoon,
                saberSpeed = cutInfo.saberSpeed,
                saberDir = cutInfo.saberDir,
                saberType = (int) cutInfo.saberType,
                timeDeviation = cutInfo.timeDeviation,
                cutDirDeviation = cutInfo.cutDirDeviation,
                cutPoint = cutInfo.cutPoint,
                cutNormal = cutInfo.cutNormal,
                cutDistanceToCenter = cutInfo.cutDistanceToCenter,
                cutAngle = cutInfo.cutAngle
            };
        }
    }
}