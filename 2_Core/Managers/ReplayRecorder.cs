using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    [UsedImplicitly]
    public class ReplayRecorder : IInitializable, IDisposable, ITickable
    {
        #region Constructor

        private readonly PlayerTransforms _playerTransforms;
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly BeatmapObjectSpawnController _beatSpawnController;
        private readonly StandardLevelScenesTransitionSetupDataSO _transitionSetup;
        private readonly ScoreController _scoreController;
        private readonly PlayerHeightDetector _playerHeightDetector;
        private readonly PauseController _pauseController;
        private AudioTimeSyncController _timeSyncController;

        private readonly Replay _replay = new();

        public ReplayRecorder(BeatmapObjectManager beatmapObjectManager) {
            _beatmapObjectManager = beatmapObjectManager;

            _beatSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
            _scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().Last();
            _playerHeightDetector = Resources.FindObjectsOfTypeAll<PlayerHeightDetector>().Last();
            _playerTransforms = Resources.FindObjectsOfTypeAll<PlayerTransforms>().Last();
            _transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();
            _pauseController = Resources.FindObjectsOfTypeAll<PauseController>().LastOrDefault();

            UserEnhancer.Enhance(_replay);
            MapEnhancer.Enhance(_replay);

            _replay.info.version = "0.0.1";
            _replay.info.gameVersion = "1.18.3";
            _replay.info.timestamp = Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            WaitForAudioTimeSyncController();
        }

        #endregion

        #region NoteIdHandling

        private readonly Dictionary<int, NoteCutInfo> _cutInfoCache = new();
        private readonly Dictionary<int, NoteEvent> _noteEventCache = new();

        private readonly Dictionary<int, SwingRatingCounterDidFinishReceiver> _finishReceiversCache = new();
        private readonly Dictionary<int, SwingRatingCounterDidChangeReceiver> _changeReceiversCache = new();

        private readonly Dictionary<NoteController, int> _noteIdCache = new();
        private int _noteId;

        private readonly Dictionary<ObstacleController, int> _wallCache = new();
        private readonly Dictionary<int, WallEvent> _wallEventCache = new();
        private int _wallId;

        #endregion

        private Pause _currentPause;
        private DateTime _pauseStartTime;

        #region Events Subscription

        public void Initialize() {
            _beatmapObjectManager.noteWasSpawnedEvent += OnNoteWasSpawned;
            _beatmapObjectManager.obstacleWasSpawnedEvent += OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent += OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent += OnNoteWasCut;
            _beatmapObjectManager.noteWasDespawnedEvent += OnNoteWasDespawned;
            _transitionSetup.didFinishEvent += OnTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent += OnBeatSpawnControllerDidInit;
            _scoreController.comboBreakingEventHappenedEvent += OnBreakCombo;
            _pauseController.didPauseEvent += OnPause;
            _pauseController.didResumeEvent += OnResume;
            if (_replay.info.height == 0)
            {
                _playerHeightDetector.playerHeightDidChangeEvent += OnPlayerHeightChange;
            }
        }

        public void Dispose() {
            _beatmapObjectManager.noteWasSpawnedEvent -= OnNoteWasSpawned;
            _beatmapObjectManager.obstacleWasSpawnedEvent -= OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent -= OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent -= OnNoteWasCut;
            _beatmapObjectManager.noteWasDespawnedEvent -= OnNoteWasDespawned;
            _transitionSetup.didFinishEvent -= OnTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent -= OnBeatSpawnControllerDidInit;
            _scoreController.comboBreakingEventHappenedEvent -= OnBreakCombo;
            _pauseController.didPauseEvent -= OnPause;
            _pauseController.didResumeEvent -= OnResume;
            if (_replay.info.height == 0)
            {
                _playerHeightDetector.playerHeightDidChangeEvent -= OnPlayerHeightChange;
            }
        }

        #endregion

        public void Tick()
        {
            if (_timeSyncController != null && _playerTransforms != null ) {
            
                Frame frame = new();
                frame.time = _timeSyncController.songTime;
                frame.fps = Mathf.RoundToInt(1.0f / Time.deltaTime);

                frame.head = new();
                frame.head.rotation = _playerTransforms.headPseudoLocalRot;
                frame.head.position = _playerTransforms.headPseudoLocalPos;

                frame.leftHand = new();
                frame.leftHand.rotation = _playerTransforms.leftHandPseudoLocalRot;
                frame.leftHand.position = _playerTransforms.leftHandPseudoLocalPos;

                frame.rightHand = new();
                frame.rightHand.rotation = _playerTransforms.rightHandPseudoLocalRot;
                frame.rightHand.position = _playerTransforms.rightHandPseudoLocalPos;

                _replay.frames.Add(frame);
            }
        }

        #region OnNoteWasSpawned

        private void OnNoteWasSpawned(NoteController noteController) {
            var noteId = _noteId++;
            _noteIdCache[noteController] = noteId;

            var noteData = noteController.noteData;

            NoteEvent noteEvent = new();
            noteEvent.noteID = noteData.lineIndex * 1000 + (int)noteData.noteLineLayer * 100 + (int)noteData.colorType * 10 + (int)noteData.cutDirection;
            noteEvent.spawnTime = _timeSyncController.songTime;
            _noteEventCache[noteId] = noteEvent;
        }

        #endregion

        #region OnObstacleWasSpawned

        private void OnObstacleWasSpawned(ObstacleController obstacleController)
        {
            var wallId = _wallId++;
            _wallCache[obstacleController] = wallId;

            var obstacleData = obstacleController.obstacleData;

            WallEvent wallEvent = new();
            wallEvent.wallID = obstacleData.lineIndex * 100 + (int)obstacleData.obstacleType * 10 + obstacleData.width;
            wallEvent.spawnTime = _timeSyncController.songTime;
            _wallEventCache[wallId] = wallEvent;
        }

        #endregion

        #region OnNoteWasMissed

        private void OnNoteWasMissed(NoteController noteController) {
            var noteId = _noteIdCache[noteController];

            if (noteController.noteData.colorType != ColorType.None)
            {
                var noteEvent = _noteEventCache[noteId];
                noteEvent.eventTime = _timeSyncController.songTime;
                noteEvent.eventType = NoteEventType.miss;
                _replay.notes.Add(noteEvent);
            }
        }

        #endregion

        #region OnBreakCombo

        private void OnBreakCombo()
        {
            PlayerHeadAndObstacleInteraction phaoi = _scoreController.GetField<PlayerHeadAndObstacleInteraction, ScoreController>("_playerHeadAndObstacleInteraction");

            if (phaoi != null && phaoi.intersectingObstacles.Count > 0)
            {
                foreach (var wall in phaoi.intersectingObstacles)
                {
                    WallEvent wallEvent = _wallEventCache[_wallCache[wall]];
                    wallEvent.time = _timeSyncController.songTime;
                    _replay.walls.Add(wallEvent);
                }
            }
        }

        #endregion

        #region OnNoteWasCut

        private void OnNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo) {
            var noteId = _noteIdCache[noteController];

            var noteEvent = _noteEventCache[noteId];
            noteEvent.eventTime = _timeSyncController.songTime;

            if (noteController.noteData.colorType == ColorType.None)
            {
                noteEvent.eventType = NoteEventType.bomb;
            }

            _replay.notes.Add(noteEvent);

            if (noteCutInfo.swingRatingCounter == null) return;
            _cutInfoCache[noteId] = noteCutInfo;

            if (noteCutInfo.speedOK && noteCutInfo.directionOK && noteCutInfo.saberTypeOK && !noteCutInfo.wasCutTooSoon) {
                noteEvent.eventType = NoteEventType.good;
            } else {
                noteEvent.eventType = NoteEventType.bad;
                PopulateNoteCutInfo(noteEvent.noteCutInfo, noteCutInfo);
            }

            var counterDidChangeReceiver = new SwingRatingCounterDidChangeReceiver(noteId, OnSwingRatingCounterDidChange);
            noteCutInfo.swingRatingCounter.RegisterDidChangeReceiver(counterDidChangeReceiver);
            _changeReceiversCache[noteId] = counterDidChangeReceiver;

            var counterDidFinishReceiver = new SwingRatingCounterDidFinishReceiver(noteId, OnSwingRatingCounterDidFinish);
            noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(counterDidFinishReceiver);
            _finishReceiversCache[noteId] = counterDidFinishReceiver;
        }

        #endregion

        #region OnSwingRatingCounterDidChange

        private void OnSwingRatingCounterDidChange(ISaberSwingRatingCounter swingRatingCounter, int noteId, float rating) {

            var cutInfo = _cutInfoCache[noteId];

            ScoreModel.RawScoreWithoutMultiplier(swingRatingCounter, cutInfo.cutDistanceToCenter,
                out var beforeCutRawScore,
                out var afterCutRawScore,
                out var cutDistanceRawScore
            );

            var cutEvent = _noteEventCache[noteId];
            var noteCutInfo = cutEvent.noteCutInfo = new();
            noteCutInfo.beforeCutRating = swingRatingCounter.beforeCutRating;
        }

        #endregion

        #region OnSwingRatingCounterDidFinish

        private void OnSwingRatingCounterDidFinish(ISaberSwingRatingCounter swingRatingCounter, int noteId) {
            swingRatingCounter.UnregisterDidChangeReceiver(_changeReceiversCache[noteId]);
            swingRatingCounter.UnregisterDidFinishReceiver(_finishReceiversCache[noteId]);

            var cutInfo = _cutInfoCache[noteId];

            ScoreModel.RawScoreWithoutMultiplier(swingRatingCounter, cutInfo.cutDistanceToCenter,
                out var beforeCutRawScore,
                out var afterCutRawScore,
                out var cutDistanceRawScore
            );

            var cutEvent = _noteEventCache[noteId];
            var noteCutInfo = cutEvent.noteCutInfo;
            PopulateNoteCutInfo(noteCutInfo, cutInfo);
            noteCutInfo.afterCutRating = swingRatingCounter.afterCutRating;
        }

        #endregion

        #region OnNoteWasDespawned

        private void OnNoteWasDespawned(NoteController noteController) {
            var noteId = _noteId++;
            _noteIdCache[noteController] = noteId;
        }

        #endregion

        #region SwingRatingCounterDidChangeReceiver

        private class SwingRatingCounterDidChangeReceiver : ISaberSwingRatingCounterDidChangeReceiver {
            private readonly Action<ISaberSwingRatingCounter, int, float> _finishEvent;
            private readonly int _noteId;

            public SwingRatingCounterDidChangeReceiver(int noteId, Action<ISaberSwingRatingCounter, int, float> finishEvent) {
                _finishEvent = finishEvent;
                _noteId = noteId;
            }

            public void HandleSaberSwingRatingCounterDidChange(ISaberSwingRatingCounter saberSwingRatingCounter, float rating) {
                _finishEvent.Invoke(saberSwingRatingCounter, _noteId, rating);
            }
        }

        #endregion

        #region SwingRatingCounterDidFinishReceiver

        private class SwingRatingCounterDidFinishReceiver : ISaberSwingRatingCounterDidFinishReceiver {
            private readonly Action<ISaberSwingRatingCounter, int> _finishEvent;
            private readonly int _noteId;

            public SwingRatingCounterDidFinishReceiver(int noteId, Action<ISaberSwingRatingCounter, int> finishEvent) {
                _finishEvent = finishEvent;
                _noteId = noteId;
            }

            public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter) {
                _finishEvent.Invoke(saberSwingRatingCounter, _noteId);
            }
        }

        #endregion

        private async void WaitForAudioTimeSyncController()
        {
            await Task.Delay(500);

            try
            {
                _timeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault(x => x.isActiveAndEnabled);
            }
            catch
            {
                Plugin.Log.Error("BSD : Could not get song length !");
            }
        }

        private void OnBeatSpawnControllerDidInit()
        {
            _replay.info.jumpDistance = _beatSpawnController.jumpDistance;
        }

        private void OnTransitionSetupOnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results)
        {
            _replay.info.score = _scoreController.prevFrameRawScore;
            switch (results.levelEndStateType)
            {
                case LevelCompletionResults.LevelEndStateType.Cleared:
                    FileManager.WriteReplay(_replay);
                    break;
                case LevelCompletionResults.LevelEndStateType.Failed:
                    if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
                        Plugin.Log.Info("Restart");
                    else
                        _replay.info.failTime = _timeSyncController.songTime;
                        FileManager.WriteReplay(_replay);
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