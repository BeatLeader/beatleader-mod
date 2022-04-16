using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Core.Managers.NoteEnhancer;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Loader;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Transform = BeatLeader.Models.Transform;

namespace BeatLeader {
    [UsedImplicitly]
    public class ReplayRecorder : IInitializable, IDisposable, ITickable {
        [Inject] [UsedImplicitly] private PlayerTransforms _playerTransforms;
        [Inject] [UsedImplicitly] private BeatmapObjectManager _beatmapObjectManager;
        [Inject] [UsedImplicitly] private BeatmapObjectSpawnController _beatSpawnController;
        [Inject] [UsedImplicitly] private StandardLevelScenesTransitionSetupDataSO _transitionSetup;
        [Inject] [UsedImplicitly] private PauseController _pauseController;
        [Inject] [UsedImplicitly] private AudioTimeSyncController _timeSyncController;
        [Inject] [UsedImplicitly] private ScoreController _scoreController;
        [Inject] [UsedImplicitly] private PlayerHeadAndObstacleInteraction _phaoi;
        [Inject] [UsedImplicitly] private GameEnergyCounter _gameEnergyCounter;
        [Inject] [UsedImplicitly] private TrackingDeviceEnhancer _trackingDeviceEnhancer;
        [Inject] [UsedImplicitly] private ScoreUtil _scoreUtil;

        private readonly PlayerHeightDetector _playerHeightDetector;
        private readonly Replay _replay = new();
        private Pause _currentPause;
        private WallEvent _currentWallEvent;
        private DateTime _pauseStartTime;
        private bool _stopRecording;

        public ReplayRecorder() {
            _playerHeightDetector = Resources.FindObjectsOfTypeAll<PlayerHeightDetector>().Last();

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
            _beatmapObjectManager.noteWasAddedEvent += OnNoteWasAdded;
            _beatmapObjectManager.obstacleWasSpawnedEvent += OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent += OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent += OnNoteWasCut;
            _transitionSetup.didFinishEvent += OnTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent += OnBeatSpawnControllerDidInit;
            _phaoi.headDidEnterObstacleEvent += OnObstacle;
            _pauseController.didPauseEvent += OnPause;
            _pauseController.didResumeEvent += OnResume;
            _scoreController.scoringForNoteFinishedEvent += OnScoringDidFinish;

            if (_replay.info.height == 0)
            {
                _playerHeightDetector.playerHeightDidChangeEvent += OnPlayerHeightChange;
            }

            ScoreUtil.EnableSubmission();
        }

        public void Dispose() {
            _beatmapObjectManager.noteWasAddedEvent -= OnNoteWasAdded;
            _beatmapObjectManager.obstacleWasSpawnedEvent -= OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent -= OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent -= OnNoteWasCut;
            _transitionSetup.didFinishEvent -= OnTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent -= OnBeatSpawnControllerDidInit;
            _phaoi.headDidEnterObstacleEvent -= OnObstacle;
            _pauseController.didPauseEvent -= OnPause;
            _pauseController.didResumeEvent -= OnResume;
            _scoreController.scoringForNoteFinishedEvent -= OnScoringDidFinish;

            if (_replay.info.height == 0)
            {
                _playerHeightDetector.playerHeightDidChangeEvent -= OnPlayerHeightChange;
            }
        }

        public void Tick() {
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
            
            var noteId = _noteId++;
            _noteIdCache[noteData] = noteId;
            NoteEvent noteEvent = new();
            noteEvent.noteID = noteData.lineIndex * 1000 + (int)noteData.noteLineLayer * 100 + (int)noteData.colorType * 10 + (int)noteData.cutDirection;
            noteEvent.spawnTime = noteData.time;
            _noteEventCache[noteId] = noteEvent;
        }

        private void OnObstacleWasSpawned(ObstacleController obstacleController)
        {
            var wallId = _wallId++;
            _wallCache[obstacleController] = wallId;

            var obstacleData = obstacleController.obstacleData;

            WallEvent wallEvent = new();
            wallEvent.wallID = obstacleData.lineIndex * 100 + (int)obstacleData.type * 10 + obstacleData.width;
            wallEvent.spawnTime = _timeSyncController.songTime;
            _wallEventCache[wallId] = wallEvent;
        }

        private void OnNoteWasMissed(NoteController noteController) {
            var noteId = _noteIdCache[noteController.noteData];

            if (noteController.noteData.colorType != ColorType.None)
            {
                var noteEvent = _noteEventCache[noteId];
                noteEvent.eventTime = _timeSyncController.songTime;
                noteEvent.eventType = NoteEventType.miss;
                _replay.notes.Add(noteEvent);
            }
        }

        private void OnObstacle(ObstacleController obstacle)
        {
            if (_currentWallEvent == null)
            {
                WallEvent wallEvent = _wallEventCache[_wallCache[obstacle]];
                wallEvent.time = _timeSyncController.songTime;
                _replay.walls.Add(wallEvent);
                _currentWallEvent = wallEvent;
            }
        }

        private void OnNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo) {
            
            var noteId = _noteIdCache[noteCutInfo.noteData];

            var noteEvent = _noteEventCache[noteId];
            noteEvent.eventTime = _timeSyncController.songTime;

            if (noteController.noteData.colorType == ColorType.None)
            {
                noteEvent.eventType = NoteEventType.bomb;
            }

            _replay.notes.Add(noteEvent);
            
            if (noteCutInfo.speedOK && noteCutInfo.directionOK && noteCutInfo.saberTypeOK && !noteCutInfo.wasCutTooSoon) {
                noteEvent.eventType = NoteEventType.good;
                noteEvent.noteCutInfo = new();
            } else {
                noteEvent.eventType = NoteEventType.bad;
                noteEvent.noteCutInfo = new();
                PopulateNoteCutInfo(noteEvent.noteCutInfo, noteCutInfo);
            }
        }

        private void OnScoringDidFinish(ScoringElement scoringElement) {
            if (scoringElement is GoodCutScoringElement goodCut) {
                CutScoreBuffer cutScoreBuffer = goodCut.GetField<CutScoreBuffer, GoodCutScoringElement>("_cutScoreBuffer");
                SaberSwingRatingCounter saberSwingRatingCounter = cutScoreBuffer.GetField<SaberSwingRatingCounter, CutScoreBuffer>("_saberSwingRatingCounter");
                
                var noteId = _noteIdCache[cutScoreBuffer.noteCutInfo.noteData];
                
                var cutEvent = _noteEventCache[noteId];
                var noteCutInfo = cutEvent.noteCutInfo;
                PopulateNoteCutInfo(noteCutInfo, cutScoreBuffer.noteCutInfo);
                SwingRatingEnhancer.Enhance(noteCutInfo, saberSwingRatingCounter);
            }
        }

        private void OnBeatSpawnControllerDidInit()
        {
            _replay.info.jumpDistance = _beatSpawnController.jumpDistance;
        }

        private void OnTransitionSetupOnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results)
        {
            _stopRecording = true;
            _replay.info.score = results.multipliedScore;
            MapEnhancer.energy = results.energy; 
            MapEnhancer.Enhance(_replay);
            _trackingDeviceEnhancer.Enhance(_replay);
            switch (results.levelEndStateType)
            {
                case LevelCompletionResults.LevelEndStateType.Cleared:
                    Plugin.Log.Info("Level Cleared. Save replay");
                    _scoreUtil.ProcessReplay(_replay);
                    break;
                case LevelCompletionResults.LevelEndStateType.Failed:
                    if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
                    {
                        Plugin.Log.Info("Restart");
                    }
                    else
                    {
                        _replay.info.failTime = _timeSyncController.songTime;
                        Plugin.Log.Info("Level Failed. Save replay");
                        _scoreUtil.ProcessReplay(_replay);
                    }
                    break;
            }

            switch (results.levelEndAction)
            {
                case LevelCompletionResults.LevelEndAction.Quit:
                    
                    break;
                case LevelCompletionResults.LevelEndAction.Restart:
                    
                    break;
            }
        }

        private void OnPlayerHeightChange(float height)
        {
            AutomaticHeight automaticHeight = new();
            automaticHeight.height = height;
            automaticHeight.time = _timeSyncController.songTime;

            _replay.heights.Add(automaticHeight);

        }

        private void OnPause()
        {
            _currentPause = new();
            _currentPause.time = _timeSyncController.songTime;
            _pauseStartTime = DateTime.Now;
        }

        private void OnResume()
        {
            _currentPause.duration = DateTime.Now.ToUnixTime() - _pauseStartTime.ToUnixTime();
            _replay.pauses.Add(_currentPause);
            _currentPause = null;
        }

        private void PopulateNoteCutInfo(Models.NoteCutInfo noteCutInfo, NoteCutInfo cutInfo) {
            noteCutInfo.speedOK = cutInfo.speedOK;
            noteCutInfo.directionOK = cutInfo.directionOK;
            noteCutInfo.saberTypeOK = cutInfo.saberTypeOK;
            noteCutInfo.wasCutTooSoon = cutInfo.wasCutTooSoon;
            noteCutInfo.saberSpeed = cutInfo.saberSpeed;
            noteCutInfo.saberDir = cutInfo.saberDir;
            noteCutInfo.saberType = (int)cutInfo.saberType;
            noteCutInfo.timeDeviation = cutInfo.timeDeviation;
            noteCutInfo.cutDirDeviation = cutInfo.cutDirDeviation;
            noteCutInfo.cutPoint = cutInfo.cutPoint;
            noteCutInfo.cutNormal = cutInfo.cutNormal;
            noteCutInfo.cutDistanceToCenter = cutInfo.cutDistanceToCenter;
            noteCutInfo.cutAngle = cutInfo.cutAngle;
        }
    }
}