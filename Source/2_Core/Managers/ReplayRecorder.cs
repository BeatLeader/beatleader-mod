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
using Quaternion = BeatLeader.Models.Replay.Quaternion;
using Transform = UnityEngine.Transform;
using Vector3 = BeatLeader.Models.Replay.Vector3;

namespace BeatLeader {
    [UsedImplicitly]
    public class ReplayRecorder : IInitializable, IDisposable, ILateTickable {
        #region Harmony

        #region Patching

        private Harmony _harmony;

        private void InitializePatches() {
            _harmony ??= new Harmony("BeatLeader.ReplayRecorder");
            _harmony.Patch(LateUpdatePatchDescriptor);
        }

        private void DisposePatches() {
            _harmony.UnpatchSelf();
        }

        #endregion

        #region ScoreController.LateUpdate Patch

        private static HarmonyPatchDescriptor LateUpdatePatchDescriptor => new(
            typeof(ScoreController).GetMethod(nameof(ScoreController.LateUpdate), BindingFlags.Instance | BindingFlags.NonPublic),
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

        #region Inject

        [Inject, UsedImplicitly]
        private SaberManager _saberManager;

        [Inject, UsedImplicitly]
        private IVRPlatformHelper _vrPlatformHelper;

        [Inject, UsedImplicitly]
        private PlayerTransforms _playerTransforms;

        [Inject, UsedImplicitly]
        private BeatmapObjectManager _beatmapObjectManager;

        [Inject, UsedImplicitly]
        private BeatmapObjectSpawnController _beatSpawnController;

        [Inject, UsedImplicitly]
        private StandardLevelScenesTransitionSetupDataSO _transitionSetup;

        [Inject, UsedImplicitly]
        private MultiplayerLevelScenesTransitionSetupDataSO _mpTransitionSetup;

        [Inject, UsedImplicitly]
        private AudioTimeSyncController _timeSyncController;

        [Inject, UsedImplicitly]
        private ScoreController _scoreController;

        [Inject, UsedImplicitly]
        private PlayerHeadAndObstacleInteraction _phaoi;

        [Inject, UsedImplicitly]
        private GameEnergyCounter _gameEnergyCounter;

        [Inject, UsedImplicitly]
        private TrackingDeviceEnhancer _trackingDeviceEnhancer;

        [InjectOptional, UsedImplicitly]
        private PlayerHeightDetector _playerHeightDetector;

        // Optional for MP support, there is no pause mechanic in multiplayer gameplay.
        [InjectOptional, UsedImplicitly]
        private PauseController _pauseController;

        #endregion

        #region Constructor

        private static ReplayRecorder? _instance;

        private readonly Replay _replay = new();
        private Pause? _currentPause;
        private WallEvent? _currentWallEvent;
        private DateTime _pauseStartTime;
        private bool _stopRecording;

        private readonly Dictionary<NoteData, int> _noteIdCache = new();
        private readonly Dictionary<int, NoteEvent> _noteEventCache = new();
        private int _noteId;

        private readonly Dictionary<ObstacleController, int> _wallCache = new();
        private readonly Dictionary<int, WallEvent> _wallEventCache = new();
        private int _wallId;

        public ReplayRecorder() {
            _instance = this;

            UserEnhancer.Enhance(_replay);

            var metaData = PluginManager.GetPluginFromId("BeatLeader");
            _replay.info.version = metaData.HVersion.ToString();
            _replay.info.gameVersion = Application.version;
            _replay.info.timestamp = Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            _stopRecording = false;
        }

        #endregion

        #region Initialize / Dispose / LateTick

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

            if (_playerHeightDetector != null) {
                _playerHeightDetector.playerHeightDidChangeEvent -= OnPlayerHeightChange;
            }

            _instance = null;
        }

        public void LateTick() {
            if (_timeSyncController == null || _playerTransforms == null || _currentPause != null || _stopRecording) return;

            RecordFrame();

            if (_currentWallEvent != null) {
                if (_phaoi != null && !_phaoi.playerHeadIsInObstacle) {
                    _currentWallEvent.energy = _gameEnergyCounter.energy;
                    _currentWallEvent = null;
                }
            }

            LazyRecordSaberOffsets();
        }

        #endregion

        #region Frames

        private Transform _origin;
        private Transform _head;
        private Transform _leftSaber;
        private Transform _rightSaber;
        private bool _framesInitialized;

        private void LazyInitFrames() {
            if (_framesInitialized) return;
            _origin = _playerTransforms._originParentTransform;
            _head = _playerTransforms._headTransform;
            _leftSaber = _saberManager.leftSaber.transform;
            _rightSaber = _saberManager.rightSaber.transform;
            _framesInitialized = true;
        }

        private void RecordFrame() {
            LazyInitFrames();

            var frame = new Frame() {
                time = _timeSyncController.songTime,
                fps = Mathf.RoundToInt(1.0f / Time.deltaTime),
                head = new Models.Replay.Transform {
                    rotation = _origin.InverseTransformRotation(_head.rotation),
                    position = _origin.InverseTransformPoint(_head.position)
                },
                leftHand = new Models.Replay.Transform {
                    rotation = _origin.InverseTransformRotation(_leftSaber.rotation),
                    position = _origin.InverseTransformPoint(_leftSaber.position)
                },
                rightHand = new Models.Replay.Transform {
                    rotation = _origin.InverseTransformRotation(_rightSaber.rotation),
                    position = _origin.InverseTransformPoint(_rightSaber.position)
                }
            };

            _replay.frames.Add(frame);
        }

        #endregion

        #region Saber Offsets

        private bool _saberOffsetsRecorded;
        private int _framesSkipped;

        private void LazyRecordSaberOffsets() {
            if (_framesSkipped++ < 10 || _saberOffsetsRecorded) return;
            TryGetSaberOffsets(_saberManager._leftSaber, out var leftLocalPos, out var leftLocalRot);
            TryGetSaberOffsets(_saberManager._rightSaber, out var rightLocalPos, out var rightLocalRot);
            _replay.saberOffsets = new SaberOffsets() {
                LeftSaberLocalPosition = leftLocalPos,
                LeftSaberLocalRotation = leftLocalRot,
                RightSaberLocalPosition = rightLocalPos,
                RightSaberLocalRotation = rightLocalRot
            };
            _saberOffsetsRecorded = true;
        }

        private void TryGetSaberOffsets(Saber saber, out Vector3 localPosition, out Quaternion localRotation) {
            localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

            var vrController = saber.gameObject.GetComponentInParent<VRController>();
            if (vrController == null) return;

            var xrRigOrigin = vrController.transform.parent;
            if (xrRigOrigin == null) return;

            var xrRigTransform = new ReeTransform(xrRigOrigin.position, xrRigOrigin.rotation);

            _vrPlatformHelper.GetNodePose(vrController._node, vrController._nodeIdx, out var controllerPos, out var controllerRot);
            controllerPos = xrRigTransform.LocalToWorldPosition(controllerPos);
            controllerRot = xrRigTransform.LocalToWorldRotation(controllerRot);
            var controllerTransform = new ReeTransform(controllerPos, controllerRot);

            localPosition = controllerTransform.WorldToLocalPosition(saber._handleTransform.position);
            localRotation = controllerTransform.WorldToLocalRotation(saber._handleTransform.rotation);
        }

        #endregion

        #region Note Events

        private void OnNoteWasAdded(NoteData noteData, BeatmapObjectSpawnMovementData.NoteSpawnData spawnData, float rotation) {
            if (_stopRecording) return;

            var noteId = _noteId++;
            _noteIdCache[noteData] = noteId;
            NoteEvent noteEvent = new() {
                noteID = ((int)noteData.scoringType + 2) * 10000
                         + noteData.lineIndex * 1000
                         + (int)noteData.noteLineLayer * 100
                         + (int)noteData.colorType * 10
                         + (int)noteData.cutDirection,
                spawnTime = noteData.time
            };
            _noteEventCache[noteId] = noteEvent;
        }

        private void OnNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo) {
            if (_stopRecording) return;

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
            if (_stopRecording) return;

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
            if (_stopRecording) return;

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

        #endregion

        #region Obstacle Events

        private void OnObstacleWasSpawned(ObstacleController obstacleController) {
            if (_stopRecording) return;

            var wallId = _wallId++;
            _wallCache[obstacleController] = wallId;

            var obstacleData = obstacleController.obstacleData;

            WallEvent wallEvent = new() {
                wallID = obstacleData.lineIndex * 100
                         + (int)obstacleData.type * 10
                         + obstacleData.width,
                spawnTime = obstacleData.time
            };
            _wallEventCache[wallId] = wallEvent;
        }

        private void OnObstacle(ObstacleController obstacle) {
            if (_stopRecording || _currentWallEvent != null) return;
            var wallEvent = _wallEventCache[_wallCache[obstacle]];
            wallEvent.time = _timeSyncController.songTime;
            _replay.walls.Add(wallEvent);
            _currentWallEvent = wallEvent;
        }

        #endregion

        #region Pause Events

        private void OnPause() {
            if (_stopRecording) return;

            _currentPause = new Pause {
                time = _timeSyncController.songTime
            };
            _pauseStartTime = DateTime.Now;
        }

        private void OnResume() {
            if (_stopRecording || _currentPause == null) return;

            _currentPause.duration = DateTime.Now.ToUnixTime() - _pauseStartTime.ToUnixTime();
            _replay.pauses.Add(_currentPause);
            _currentPause = null;
        }

        #endregion

        #region Misc. Events

        private void OnBeatSpawnControllerDidInit() {
            _replay.info.jumpDistance = _beatSpawnController.jumpDistance;
        }

        private void OnPlayerHeightChange(float height) {
            if (_stopRecording) return;

            AutomaticHeight automaticHeight = new();
            automaticHeight.height = height;
            automaticHeight.time = _timeSyncController.songTime;

            _replay.heights.Add(automaticHeight);
        }

        #endregion

        #region Custom Data

        [PublicAPI]
        public static bool TryWriteCustomDataStatic(string key, byte[] data) {
            return _instance != null && _instance.TryWriteCustomData(key, data);
        }

        [PublicAPI]
        public bool TryWriteCustomData(string key, byte[] data) {
            if (_stopRecording) return false;
            _replay.customData[key] = data;
            return true;
        }

        #endregion

        #region OnFinish

        [PublicAPI]
        public event Action OnFinalizeReplay;

        private void OnTransitionSetupOnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results) {
            FinalizeReplay(results);
        }

        private void OnMultiplayerTransitionSetupOnDidFinishEvent(MultiplayerLevelScenesTransitionSetupDataSO data, MultiplayerResultsData results) {
            var mpResults = results?.localPlayerResultData?.multiplayerLevelCompletionResults;
            if (_replay == null || mpResults == null) return;
            if (mpResults.playerLevelEndState != MultiplayerLevelCompletionResults.MultiplayerPlayerLevelEndState.SongFinished) return;

            FinalizeReplay(mpResults.levelCompletionResults);
        }

        private void FinalizeReplay(LevelCompletionResults results) {
            _replay.notes.RemoveAll(note => note.eventType == NoteEventType.unknown);

            _replay.info.score = results.multipliedScore;
            MapEnhancer.energy = results.energy;
            MapEnhancer.Enhance(_replay);
            _trackingDeviceEnhancer.Enhance(_replay);

            PlayEndData playEndData = new(results);
            if (playEndData.EndType == LevelEndType.Fail) {
                _replay.info.failTime = _timeSyncController.songTime;
            }

            try {
                OnFinalizeReplay?.Invoke();
            } catch (Exception ex) {
                Plugin.Log.Error($"OnFinalizeReplay exception: {ex}");
            }

            _stopRecording = true;

            Plugin.Log.Debug($"Level result: {playEndData.EndType}, end time: {playEndData.Time}");
            if (_replay.notes.Count > 0) {
                ScoreUtil.ProcessReplay(_replay, playEndData);
            } else {
                Plugin.Log.Debug("Not enough notes to submit");
            }
        }

        #endregion
    }
}