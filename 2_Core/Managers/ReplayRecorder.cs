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

        private readonly GameScenesManager _gameScenesManager;
        private readonly SaberManager _saberManager;
        private readonly PlayerTransforms _playerTransforms;
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly GameplayModifiersModelSO _modifierData;
        private readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private readonly PlayerDataModel _playerData;
        private readonly BeatmapObjectSpawnController _beatSpawnController;
        private StandardLevelScenesTransitionSetupDataSO _transitionSetup;
        private readonly ScoreController _scoreController;
        private readonly PlayerHeightDetector _playerHeightDetector;
        private AudioTimeSyncController _timeSyncController;

        private readonly Replay _replay = new();

        public ReplayRecorder(BeatmapObjectManager beatmapObjectManager) {
            _beatmapObjectManager = beatmapObjectManager;

            
            _beatSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
            _scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().Last();
            _playerHeightDetector = Resources.FindObjectsOfTypeAll<PlayerHeightDetector>().Last();
            _saberManager = Resources.FindObjectsOfTypeAll<SaberManager>().Last();
            _playerTransforms = Resources.FindObjectsOfTypeAll<PlayerTransforms>().Last();
            _gameScenesManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();
            _transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();

            Plugin.Log.Info($"Transition setup" + _transitionSetup.ToString());

            _transitionSetup.didFinishEvent += OnTransitionSetupOnDidFinishEvent;


            UserEnhancer.Enhance(_replay);
            MapEnhancer.Enhance(_replay);

            _replay.info.version = "0.0.1";
            _replay.info.gameVersion = "1.18.3";

            WaitForAudioTimeSyncController();
        }

        #endregion

        #region NoteIdHandling

        private readonly Dictionary<int, NoteCutInfo> _cutInfoCache = new();
        private readonly Dictionary<int, NoteCutEvent> _cutEventCache = new();

        private readonly Dictionary<int, SwingRatingCounterDidFinishReceiver> _finishReceiversCache = new();
        private readonly Dictionary<int, SwingRatingCounterDidChangeReceiver> _changeReceiversCache = new();

        private readonly Dictionary<NoteController, int> _noteIdCache = new();
        private int _noteId;

        private readonly Dictionary<ObstacleController, int> _wallCache = new();
        private readonly Dictionary<int, WallEvent> _wallEventCache = new();
        private int _wallId;

        #endregion

        #region Events Subscription

        public void Initialize() {
            _gameScenesManager.transitionDidFinishEvent += OnGameSceneLoaded;
            _saberManager.didUpdateSaberPositionsEvent += OnDidUpdateSaberPositions;
            _beatmapObjectManager.noteWasSpawnedEvent += OnNoteWasSpawned;
            _beatmapObjectManager.obstacleWasSpawnedEvent += OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent += OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent += OnNoteWasCut;
            _beatmapObjectManager.noteWasDespawnedEvent += OnNoteWasDespawned;
            
            _beatSpawnController.didInitEvent += OnBeatSpawnControllerDidInit;
            _scoreController.comboBreakingEventHappenedEvent += OnBreakCombo;
            if (_replay.info.height == 0)
            {
                _playerHeightDetector.playerHeightDidChangeEvent += OnPlayerHeightChange;
            }
        }

        public void Dispose() {
            _gameScenesManager.transitionDidFinishEvent -= OnGameSceneLoaded;
            _saberManager.didUpdateSaberPositionsEvent -= OnDidUpdateSaberPositions;
            _beatmapObjectManager.noteWasSpawnedEvent -= OnNoteWasSpawned;
            _beatmapObjectManager.obstacleWasSpawnedEvent -= OnObstacleWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent -= OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent -= OnNoteWasCut;
            _beatmapObjectManager.noteWasDespawnedEvent -= OnNoteWasDespawned;
            _transitionSetup.didFinishEvent -= OnTransitionSetupOnDidFinishEvent;
            _beatSpawnController.didInitEvent -= OnBeatSpawnControllerDidInit;
            _scoreController.comboBreakingEventHappenedEvent -= OnBreakCombo;
            if (_replay.info.height == 0)
            {
                _playerHeightDetector.playerHeightDidChangeEvent -= OnPlayerHeightChange;
            }
        }

        #endregion

        public void Tick()
        {
            if (_timeSyncController != null && _playerTransforms != null ) {
            
                Frame headFrame = new();
                headFrame.time = _timeSyncController.songTime;
                headFrame.transform = new();
                headFrame.transform.rotation = _playerTransforms.headPseudoLocalRot.eulerAngles;
                headFrame.transform.position = _playerTransforms.headPseudoLocalPos;

                _replay.head.Add(headFrame);
            }
        }

        private void OnGameSceneLoaded(ScenesTransitionSetupDataSO transitionSetupData, DiContainer diContainer)
        {
            Plugin.Log.Info($"OnGameSceneLoaded");
            _gameScenesManager.transitionDidFinishEvent -= OnGameSceneLoaded;
            _transitionSetup = Resources.FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>().FirstOrDefault();

            Plugin.Log.Info($"Transition setup" + _transitionSetup.ToString());

            _transitionSetup.didFinishEvent += OnTransitionSetupOnDidFinishEvent;
        }

        #region OnNoteWasSpawned

        private void OnNoteWasSpawned(NoteController noteController) {
            var noteId = _noteId++;
            _noteIdCache[noteController] = noteId;

            var noteData = noteController.noteData;

            NoteCutEvent cutEvent = new();
            cutEvent.noteHash = noteData.lineIndex ^ (int)noteData.noteLineLayer ^ (int)noteData.colorType ^ (int)noteData.cutDirection;
            cutEvent.spawnTime = _timeSyncController.songTime;
            _cutEventCache[noteId] = cutEvent;

            //Plugin.Log.Info($"Note_{noteId} was spawned!");
        }

        #endregion

        #region OnObstacleWasSpawned

        private void OnObstacleWasSpawned(ObstacleController obstacleController)
        {
            var wallId = _wallId++;
            _wallCache[obstacleController] = wallId;

            var obstacleData = obstacleController.obstacleData;

            WallEvent wallEvent = new();
            wallEvent.wallHash = obstacleData.lineIndex ^ (int)obstacleData.obstacleType ^ obstacleData.width;
            wallEvent.spawnTime = _timeSyncController.songTime;
            _wallEventCache[wallId] = wallEvent;

            //Plugin.Log.Info($"Wall_{_wallId} was spawned!");
        }

        #endregion

        #region OnNoteWasMissed

        private void OnNoteWasMissed(NoteController noteController) {
            var noteId = _noteIdCache[noteController];

            if (noteController.noteData.colorType != ColorType.None)
            {
                var cutEvent = _cutEventCache[noteId];

                NoteMissEvent missEvent = new();
                missEvent.spawnTime = cutEvent.spawnTime;
                missEvent.noteHash = cutEvent.noteHash;
                missEvent.missTime = _timeSyncController.songTime;
                _replay.misses.Add(missEvent);
            }

            //Plugin.Log.Info($"Note_{noteId} was missed!");
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

            //Plugin.Log.Info($"Note_{noteId} was cut!");

            if (noteCutInfo.swingRatingCounter == null) return;
            _cutInfoCache[noteId] = noteCutInfo;

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
            //Plugin.Log.Info($"Note_{noteId} post-swing rating changed to {rating}!");

            var cutInfo = _cutInfoCache[noteId];

            ScoreModel.RawScoreWithoutMultiplier(swingRatingCounter, cutInfo.cutDistanceToCenter,
                out var beforeCutRawScore,
                out var afterCutRawScore,
                out var cutDistanceRawScore
            );

            var cutEvent = _cutEventCache[noteId];
            var noteCutInfo = cutEvent.noteCutInfo = new();
            noteCutInfo.beforeCutRating = swingRatingCounter.beforeCutRating;

            //Plugin.Log.Info($"pre: {beforeCutRawScore} acc: {cutDistanceRawScore} post: {afterCutRawScore}");
        }

        #endregion

        #region OnSwingRatingCounterDidFinish

        private void OnSwingRatingCounterDidFinish(ISaberSwingRatingCounter swingRatingCounter, int noteId) {
            swingRatingCounter.UnregisterDidChangeReceiver(_changeReceiversCache[noteId]);
            swingRatingCounter.UnregisterDidFinishReceiver(_finishReceiversCache[noteId]);

            //Plugin.Log.Info($"Note_{noteId} post-swing rating finished!");

            var cutInfo = _cutInfoCache[noteId];

            ScoreModel.RawScoreWithoutMultiplier(swingRatingCounter, cutInfo.cutDistanceToCenter,
                out var beforeCutRawScore,
                out var afterCutRawScore,
                out var cutDistanceRawScore
            );

            var cutEvent = _cutEventCache[noteId];
            var noteCutInfo = cutEvent.noteCutInfo;

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
            noteCutInfo.afterCutRating = swingRatingCounter.afterCutRating;

            _replay.cuts.Add(cutEvent);

           // Plugin.Log.Info($"pre: {beforeCutRawScore} acc: {cutDistanceRawScore} post: {afterCutRawScore}");
        }

        #endregion

        #region OnNoteWasDespawned

        private void OnNoteWasDespawned(NoteController noteController) {
            var noteId = _noteId++;
            _noteIdCache[noteController] = noteId;

            //Plugin.Log.Info($"Note_{noteId} was Despawned!");
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

        private void OnDidUpdateSaberPositions(Saber left, Saber right)
        {
            if (left != null && _timeSyncController != null)
            {
                Frame leftSaber = new();
                leftSaber.time = _timeSyncController.songTime;
                leftSaber.transform = new();
                leftSaber.transform.rotation = left.transform.eulerAngles;
                leftSaber.transform.position = left.transform.position;
                _replay.left.Add(leftSaber);
            }
            
            if (right != null && _timeSyncController != null)
            {
                Frame rightSaber = new();
                rightSaber.time = _timeSyncController.songTime;
                rightSaber.transform = new();
                rightSaber.transform.rotation = left.transform.eulerAngles;
                rightSaber.transform.position = left.transform.position;
                _replay.right.Add(rightSaber);
            }
        }

        private void OnBeatSpawnControllerDidInit()
        {
            _replay.info.jumpDistance = _beatSpawnController.jumpDistance;
        }

        private void OnTransitionSetupOnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results)
        {
            Plugin.Log.Info($"OnTransitionSetupOnDidFinishEvent");
            switch (results.levelEndStateType)
            {
                case LevelCompletionResults.LevelEndStateType.Cleared:
                    Plugin.Log.Info("Level finished");
                    FileManager.WriteReplay(_replay);
                    break;
                case LevelCompletionResults.LevelEndStateType.Failed:
                    if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
                        Plugin.Log.Info("Restart");
                    else
                        _replay.info.failTime = _timeSyncController.songTime;
                        Plugin.Log.Info("Failed");
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

            _replay.height.Add(automaticHeight);

        }
    }
}