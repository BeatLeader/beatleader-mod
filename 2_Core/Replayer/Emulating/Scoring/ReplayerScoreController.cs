using System;
using System.Linq;
using System.Collections.Generic;
using static ScoreMultiplierCounter;
using BeatLeader.Replayer.Emulating;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using System.Reflection;

namespace BeatLeader.Replayer.Scoring
{
    public class ReplayerScoreController : ScoreController, IReplayerScoreController
    {
        [Inject] private readonly Replay _replay;
        [Inject] private readonly SimpleNoteComparatorsSpawner _simpleNoteComparatorsSpawner;
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly IScoringInterlayer _scoringInterlayer;
        [Inject] private readonly IReadonlyBeatmapData _beatmapData;

        private int _maxComboAfterRescoring;
        private int _comboAfterRescoring;

        private FieldInfo _fieldInfo_scoringForNoteStartedEvent;
        private MethodInfo _methodInfo_scoringForNoteStartedEvent;

        private FieldInfo _fieldInfo_scoreDidChangeEvent;
        private MethodInfo _methodInfo_scoreDidChangeEvent;

        private FieldInfo _fieldInfo_multiplierDidChangeEvent;
        private MethodInfo _methodInfo_multiplierDidChangeEvent;

        public int MaxComboAfterRescoring => _maxComboAfterRescoring;
        public int ComboAfterRescoring => _comboAfterRescoring;

        public event Action<int, int, bool> OnComboChangedAfterRescoring;

        public override void Start()
        {
            // Reimplement the full constructor because calling base.Start doesn't make it work, too tired. HardCPP.
            _gameplayModifiersModel = Resources.FindObjectsOfTypeAll<GameplayModifiersModelSO>().First();
            _gameplayModifierParams = _gameplayModifiersModel.CreateModifierParamsList(_gameplayModifiers);
            _prevMultiplierFromModifiers = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent += HandlePlayerHeadDidEnterObstacles;
            _beatmapObjectManager.noteWasCutEvent += HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            _beatmapTimeController.OnSongRewind += RescoreInTimeSpan;

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            _fieldInfo_scoringForNoteStartedEvent = typeof(ScoreController).GetField("scoringForNoteStartedEvent", bindingFlags);
            _fieldInfo_scoreDidChangeEvent = typeof(ScoreController).GetField("scoreDidChangeEvent", bindingFlags);
            _fieldInfo_multiplierDidChangeEvent = typeof(ScoreController).GetField("multiplierDidChangeEvent", bindingFlags);
        }
        public override void OnDestroy()
        {
            // Reimplement the full constructor because calling base.OnDestroy doesn't make it work, too tired. HardCPP.
            if (_playerHeadAndObstacleInteraction != null)
            {
                _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent -= HandlePlayerHeadDidEnterObstacles;
            }

            if (_beatmapObjectManager != null)
            {
                _beatmapObjectManager.noteWasCutEvent -= HandleNoteWasCut;
                _beatmapObjectManager.noteWasMissedEvent -= HandleNoteWasMissed;
                _beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
            }

            if (_beatmapTimeController != null)
            {
                _beatmapTimeController.OnSongRewind -= RescoreInTimeSpan;
            }
        }

        public virtual void RescoreInTimeSpan(float endTime)
        {
            List<BeatmapDataItem> filteredBeatmapItems = _beatmapData
               .GetFilteredCopy(x => x.time >= _audioTimeSyncController.songTimeOffset && x.time <= endTime
               && x.type == BeatmapDataItem.BeatmapDataItemType.BeatmapObject ? x : null).allBeatmapDataItems.ToList();

            _modifiedScore = 0;
            _multipliedScore = 0;
            _immediateMaxPossibleModifiedScore = 0;
            _immediateMaxPossibleMultipliedScore = 0;
            _comboAfterRescoring = 0;
            _maxComboAfterRescoring = 0;
            _scoreMultiplierCounter.Reset();
            _maxScoreMultiplierCounter.Reset();

            bool broke = false;
            foreach (BeatmapDataItem item in filteredBeatmapItems)
            {
                NoteData noteData;
                NoteEvent noteEvent;
                if ((noteData = item as NoteData) != null && (noteEvent = noteData.GetNoteEvent(_replay)) != null)
                {
                    switch (noteEvent.eventType)
                    {
                        case NoteEventType.good:
                            {
                                _scoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Positive);
                                if (noteData.ComputeNoteMultiplier() == MultiplierEventType.Positive)
                                    _maxScoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Positive);

                                int totalScore = noteEvent.ComputeNoteScore();
                                int maxPossibleScore = ScoreModel.GetNoteScoreDefinition(noteData.scoringType).maxCutScore;

                                _multipliedScore += totalScore * _scoreMultiplierCounter.multiplier;
                                _immediateMaxPossibleMultipliedScore += maxPossibleScore * _maxScoreMultiplierCounter.multiplier;
                                _comboAfterRescoring++;
                                _maxComboAfterRescoring = _comboAfterRescoring > _maxComboAfterRescoring ? _comboAfterRescoring : _maxComboAfterRescoring;

                                float totalMultiplier = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
                                _prevMultiplierFromModifiers = _prevMultiplierFromModifiers != totalMultiplier ? totalMultiplier : _prevMultiplierFromModifiers;

                                _modifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(_multipliedScore, totalMultiplier);
                                _immediateMaxPossibleModifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(_immediateMaxPossibleMultipliedScore, totalMultiplier);
                            }
                            break;
                        case NoteEventType.bad:
                        case NoteEventType.miss:
                            _scoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Negative);
                            _comboAfterRescoring = 0;
                            broke = true;
                            break;
                        case NoteEventType.bomb:
                            _scoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Negative);
                            break;
                        default: throw new Exception("Unknown note type exception!");
                    }
                    continue;
                }

                ObstacleData obstacleData;
                WallEvent wallEvent;
                if ((obstacleData = item as ObstacleData) == null || (wallEvent = obstacleData.GetWallEvent(_replay)) == null) continue;

                _scoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Negative);
                _comboAfterRescoring = 0;
                broke = true;
            }

            OnComboChangedAfterRescoring?.Invoke(_comboAfterRescoring, _maxComboAfterRescoring, broke);

            //scoreDidChangeEvent?.Invoke(_multipliedScore, _modifiedScore);
            {
                var eventDelegate = _fieldInfo_scoreDidChangeEvent.GetValue(this);
                if (eventDelegate != null)
                {
                    if (_methodInfo_scoreDidChangeEvent == null) _methodInfo_scoreDidChangeEvent = eventDelegate.GetType().GetMethod("Invoke");
                    _methodInfo_scoreDidChangeEvent.Invoke(eventDelegate, new object[] { _multipliedScore, _modifiedScore });
                }
            }

            //multiplierDidChangeEvent?.Invoke(_scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress);
            {
                var eventDelegate = _fieldInfo_multiplierDidChangeEvent.GetValue(this);
                if (eventDelegate != null)
                {
                    if (_methodInfo_multiplierDidChangeEvent == null) _methodInfo_multiplierDidChangeEvent = eventDelegate.GetType().GetMethod("Invoke");
                    _methodInfo_multiplierDidChangeEvent.Invoke(eventDelegate, new object[] { _scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress });
                }
            }

        }
        public override void HandleNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteCutInfo.noteData.scoringType == NoteData.ScoringType.Ignore) return;
            if (noteCutInfo.allIsOK)
            {
                ScoringData scoringData = new ScoringData();
                if (_simpleNoteComparatorsSpawner != null && _simpleNoteComparatorsSpawner
                    .TryGetLoadedComparator(noteController, out SimpleNoteCutComparator comparator))
                {
                    scoringData = new ScoringData(comparator.NoteController.noteData, comparator.NoteCutEvent,
                        comparator.NoteController.worldRotation, comparator.NoteController.inverseWorldRotation,
                        comparator.NoteController.noteTransform.localRotation, comparator.NoteController.noteTransform.position);
                    comparator.Dispose();
                }
                else
                    scoringData = new ScoringData(noteController, noteController.GetNoteEvent(_replay));

                ScoringElement scoringElement = _scoringInterlayer.Convert<GoodCutScoringElement>(scoringData);
                ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, scoringElement);

                //scoringForNoteStartedEvent?.Invoke(scoringElement);
                var eventDelegate = _fieldInfo_scoringForNoteStartedEvent.GetValue(this);
                if (eventDelegate != null)
                {
                    if (_methodInfo_scoringForNoteStartedEvent == null) _methodInfo_scoringForNoteStartedEvent = eventDelegate.GetType().GetMethod("Invoke");
                    _methodInfo_scoringForNoteStartedEvent.Invoke(eventDelegate, new object[] { scoringElement });
                }


                _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
            }
            else
            {
                BadCutScoringElement badCutScoringElement = _badCutScoringElementPool.Spawn();
                badCutScoringElement.Init(noteCutInfo.noteData);
                ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, badCutScoringElement);

                //scoringForNoteStartedEvent?.Invoke(badCutScoringElement);
                var eventDelegate = _fieldInfo_scoringForNoteStartedEvent.GetValue(this);
                if (eventDelegate != null)
                {
                    if (_methodInfo_scoringForNoteStartedEvent == null) _methodInfo_scoringForNoteStartedEvent = eventDelegate.GetType().GetMethod("Invoke");
                    _methodInfo_scoringForNoteStartedEvent.Invoke(eventDelegate, new object[] { badCutScoringElement });
                }

                _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
            }
        }
    }
}
