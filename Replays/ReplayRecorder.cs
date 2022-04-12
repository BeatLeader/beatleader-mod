using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.Replays.ReplayEnhancers;
using BeatLeader.Replays.Models;
using BeatLeader.Utils;
using HarmonyLib;
using IPA.Loader;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using static BeatLeader.Replays.Models.Replay;
using ReplayTransform = BeatLeader.Replays.Models.Replay.Transform;

namespace BeatLeader.Replays
{
    public class ReplayRecorder : MonoBehaviour
    {
        [Inject] private protected readonly ReplayDataProvider _replayDataProvider;
        [Inject] private protected readonly ScoreUtil _scoreUtil;

        private List<PauseInfo> _tempPauses;
        private Dictionary<int, (NoteData, NoteEvent)> _tempNoteEvents;
        private Dictionary<int, (ObstacleController, WallEvent)> _tempWallEvents;

        private protected Replay _tempReplay;
        private protected bool _isRecording;
        private bool _autoStop;

        private WallEvent currentWallEvent;
        private NoteEvent currentNoteEvent;

        public Replay tempReplay => _tempReplay;
        public bool isRecording => _isRecording;

        public void Init(bool autoStop = true)
        {
            Init(autoStop, null);
        }
        public void Init(bool autoStop, in Replay replayToModify)
        {
            _tempReplay = replayToModify != null ? replayToModify : new Replay();

            UserEnhancer.Enhance(_tempReplay);
            _tempReplay.info.version = PluginManager.GetPluginFromId("BeatLeader").HVersion.ToString();
            _tempReplay.info.gameVersion = Application.version;
            _tempReplay.info.timestamp = Convert.ToString((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            _tempPauses = new List<PauseInfo>();
            _tempNoteEvents = new Dictionary<int, (NoteData, NoteEvent)>();
            _tempWallEvents = new Dictionary<int, (ObstacleController, WallEvent)>();

            _autoStop = autoStop;
            _isRecording = true;
        }

        public void Update()
        {
            if (!_isRecording) return;

            Frame frame = new Frame()
            {
                time = _replayDataProvider.timeSyncController.songTime,
                fps = Mathf.RoundToInt(1.0f / Time.deltaTime),
                head = new ReplayTransform(_replayDataProvider.playerTransforms.headPseudoLocalPos, _replayDataProvider.playerTransforms.headPseudoLocalRot),
                leftHand = new ReplayTransform(_replayDataProvider.playerTransforms.leftHandPseudoLocalPos, _replayDataProvider.playerTransforms.leftHandPseudoLocalRot),
                rightHand = new ReplayTransform(_replayDataProvider.playerTransforms.rightHandPseudoLocalPos, _replayDataProvider.playerTransforms.rightHandPseudoLocalRot)
            };

            _tempReplay.frames.Add(frame);

            if (currentWallEvent != null)
            {
                if (!_replayDataProvider.playerHeadAndObstacleInteraction.playerHeadIsInObstacle)
                {
                    currentWallEvent.energy = _replayDataProvider.gameEnergyCounter.energy;
                    currentWallEvent = null;
                }
            }
        }
        private protected void SubscribeEvents()
        {
            _replayDataProvider.beatmapObjectManager.noteWasAddedEvent -= OnNoteWasAdded;
            _replayDataProvider.beatmapObjectManager.obstacleWasSpawnedEvent -= OnObstacleWasSpawned;
            _replayDataProvider.beatmapObjectManager.noteWasMissedEvent -= OnNoteWasMissed;
            _replayDataProvider.beatmapObjectManager.noteWasCutEvent -= OnNoteWasCut;
            _replayDataProvider.standardLevelScenesTransitionData.didFinishEvent -= OnTransitionSetupOnDidFinishEvent;
            _replayDataProvider.beatmapObjectSpawnController.didInitEvent -= OnBeatSpawnControllerDidInit;
            _replayDataProvider.playerHeadAndObstacleInteraction.headDidEnterObstacleEvent -= OnObstacleWithHeadInteraction;
            _replayDataProvider.pauseController.didPauseEvent -= OnPause;
            _replayDataProvider.pauseController.didResumeEvent -= OnResume;
            _replayDataProvider.scoreController.scoringForNoteFinishedEvent -= OnScoringDidFinish;
            _replayDataProvider.playerHeightDetector.playerHeightDidChangeEvent -= OnPlayerHeightChange;
        }
        private protected void UnsubscribeEvents()
        {
            _replayDataProvider.beatmapObjectManager.noteWasAddedEvent += OnNoteWasAdded;
            _replayDataProvider.beatmapObjectManager.obstacleWasSpawnedEvent += OnObstacleWasSpawned;
            _replayDataProvider.beatmapObjectManager.noteWasMissedEvent += OnNoteWasMissed;
            _replayDataProvider.beatmapObjectManager.noteWasCutEvent += OnNoteWasCut;
            _replayDataProvider.standardLevelScenesTransitionData.didFinishEvent += OnTransitionSetupOnDidFinishEvent;
            _replayDataProvider.beatmapObjectSpawnController.didInitEvent += OnBeatSpawnControllerDidInit;
            _replayDataProvider.playerHeadAndObstacleInteraction.headDidEnterObstacleEvent += OnObstacleWithHeadInteraction;
            _replayDataProvider.pauseController.didPauseEvent += OnPause;
            _replayDataProvider.pauseController.didResumeEvent += OnResume;
            _replayDataProvider.scoreController.scoringForNoteFinishedEvent += OnScoringDidFinish;
            _replayDataProvider.playerHeightDetector.playerHeightDidChangeEvent += OnPlayerHeightChange;
        }

        private void OnPause()
        {
            PauseInfo pause = new PauseInfo();
            pause.pauseTime = _replayDataProvider.timeSyncController.songTime;
            _tempPauses.Add(pause);
        }
        private void OnResume()
        {
            PauseInfo pause = _tempPauses[_tempPauses.Count];
            if (pause.pauseTime > _replayDataProvider.timeSyncController.songTime)
            {
                pause.unPauseTime = _replayDataProvider.timeSyncController.songTime;
            }
            else _tempPauses.Remove(pause);
        }
        private void OnNoteWasAdded(NoteData noteData, BeatmapObjectSpawnMovementData.NoteSpawnData spawnData, float rotation)
        {
            NoteEvent noteEvent = new NoteEvent();
            noteEvent.noteID = noteData.ComputeNoteID();
            noteEvent.spawnTime = noteData.time;
            _tempNoteEvents.Add(noteEvent.noteID, (noteData, noteEvent));
        }
        private void OnObstacleWasSpawned(ObstacleController obstacleController)
        {
            WallEvent wallEvent = new();
            wallEvent.wallID = obstacleController.obstacleData.ComputeObstacleID();
            wallEvent.spawnTime = _replayDataProvider.timeSyncController.songTime;
            _tempWallEvents.Add(wallEvent.wallID, (obstacleController, wallEvent));
        }
        private void OnNoteWasMissed(NoteController noteController)
        {
            if (noteController.noteData.colorType != ColorType.None)
            {
                var noteEvent = _tempNoteEvents[noteController.noteData.ComputeNoteID()].Item2;
                noteEvent.eventTime = _replayDataProvider.timeSyncController.songTime;
                noteEvent.eventType = NoteEventType.miss;
                _tempReplay.AddItemToReplay(noteEvent);
            }
        }
        private void OnObstacleWithHeadInteraction(ObstacleController obstacle)
        {
            if (currentWallEvent == null)
            {
                WallEvent wallEvent = new WallEvent();
                wallEvent.time = _replayDataProvider.timeSyncController.songTime;
                _tempReplay.AddItemToReplay(wallEvent);
                currentWallEvent = wallEvent;
            }
        }
        private void OnNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            var noteEvent = _tempNoteEvents[noteController.noteData.ComputeNoteID()].Item2;
            noteEvent.eventTime = _replayDataProvider.timeSyncController.songTime;

            if (noteController.noteData.colorType == ColorType.None)
            {
                noteEvent.eventType = NoteEventType.bomb;
            }

            noteEvent.noteCutInfo = noteCutInfo;
            if (noteCutInfo.speedOK && noteCutInfo.directionOK && noteCutInfo.saberTypeOK && !noteCutInfo.wasCutTooSoon)
            {
                noteEvent.eventType = NoteEventType.good;
            }
            else
            {
                noteEvent.eventType = NoteEventType.bad;
            }

            _tempReplay.AddItemToReplay(noteEvent);
        }
        private void OnScoringDidFinish(ScoringElement scoringElement)
        {
            if (scoringElement is GoodCutScoringElement goodCut)
            {
                var cutEvent = _tempNoteEvents[scoringElement.noteData.ComputeNoteID()].Item2;
                var noteCutInfo = cutEvent.noteCutInfo;
                noteCutInfo = goodCut.cutScoreBuffer.noteCutInfo;
                noteCutInfo.beforeCutRating = goodCut.cutScoreBuffer.beforeCutSwingRating;
                noteCutInfo.afterCutRating = goodCut.cutScoreBuffer.afterCutSwingRating;
            }
        }
        private void OnPlayerHeightChange(float height)
        {
            HeightInfo heightInfo = new HeightInfo();
            heightInfo.height = height;
            heightInfo.time = _replayDataProvider.timeSyncController.songTime;

            _tempReplay.AddItemToReplay(heightInfo);
        }

        private void OnBeatSpawnControllerDidInit()
        {
            _tempReplay.info.jumpDistance = _replayDataProvider.beatmapObjectSpawnController.jumpDistance;
        }
        private void OnTransitionSetupOnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults results)
        {
            if (!_autoStop) return;

            _tempReplay.info.score = results.multipliedScore;
            MapEnhancer.energy = results.energy;

            MapEnhancer.Enhance(_tempReplay);
            _replayDataProvider.trackingDeviceEnhancer.Enhance(_tempReplay);

            switch (results.levelEndStateType)
            {
                case LevelCompletionResults.LevelEndStateType.Cleared:
                    Plugin.Log.Info("Level Cleared. Saving replay");
                    ScoreUtil.ProcessReplay(_tempReplay);
                    break;
                case LevelCompletionResults.LevelEndStateType.Failed:
                    if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
                    {
                        Plugin.Log.Info("Restart");
                    }
                    else
                    {
                        _tempReplay.info.failTime = _replayDataProvider.timeSyncController.songTime;
                        Plugin.Log.Info("Level Failed. Saving replay");
                        ScoreUtil.ProcessReplay(_tempReplay);
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
            Destroy(this);
        }
    }
}