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
        [Inject] private readonly ReplayLaunchData _replayData;
        [Inject] private readonly SimpleNoteComparatorsSpawner _simpleNoteComparatorsSpawner;
        [InjectOptional] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly IScoringInterlayer _scoringInterlayer;
        [Inject] private readonly IReadonlyBeatmapData _beatmapData;

        private int _maxComboAfterRescoring;
        private int _comboAfterRescoring;

        public int MaxComboAfterRescoring => _maxComboAfterRescoring;
        public int ComboAfterRescoring => _comboAfterRescoring;

        public event Action<int, int, bool> OnComboChangedAfterRescoring;

        public override void Start()
        {
            _gameplayModifiersModel = Resources.FindObjectsOfTypeAll<GameplayModifiersModelSO>().First();
            _gameplayModifierParams = _gameplayModifiersModel.CreateModifierParamsList(_gameplayModifiers);
            _prevMultiplierFromModifiers = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent += HandlePlayerHeadDidEnterObstacles;
            _beatmapObjectManager.noteWasCutEvent += HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            if (_beatmapTimeController != null)
                _beatmapTimeController.OnSongRewind += RescoreInTimeSpan;
            Reflect();
        }
        public override void OnDestroy()
        {
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
                if ((noteData = item as NoteData) != null && (noteEvent = noteData.GetNoteEvent(_replayData.replay)) != null)
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
                if ((obstacleData = item as ObstacleData) == null || (wallEvent = obstacleData.GetWallEvent(_replayData.replay)) == null) continue;

                _scoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Negative);
                _comboAfterRescoring = 0;
                broke = true;
            }

            OnComboChangedAfterRescoring?.Invoke(_comboAfterRescoring, _maxComboAfterRescoring, broke);
            InvokeScoreDidChange(_multipliedScore, _modifiedScore);
            InvokeMultiplierDidChange(_scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress);
        }
        public override void HandleNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteCutInfo.noteData.scoringType == NoteData.ScoringType.Ignore) return;
            if (noteCutInfo.allIsOK)
            {
                ScoringData scoringData;
                if (_simpleNoteComparatorsSpawner != null && _simpleNoteComparatorsSpawner
                    .TryGetLoadedComparator(noteController, out SimpleNoteCutComparator comparator))
                {
                    scoringData = new ScoringData(comparator.NoteController, comparator.NoteEvent);
                    comparator.Dispose();
                }
                else
                    scoringData = new ScoringData(noteController, noteController.GetNoteEvent(_replayData.replay));

                ScoringElement scoringElement = _scoringInterlayer.Convert<GoodCutScoringElement>(scoringData);
                ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, scoringElement);
                InvokeScoringStarted(scoringElement);
                _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
            }
            else
            {
                BadCutScoringElement badCutScoringElement = _badCutScoringElementPool.Spawn();
                badCutScoringElement.Init(noteCutInfo.noteData);
                ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, badCutScoringElement);
                InvokeScoringStarted(badCutScoringElement);
                _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
            }
        }

        #region EventsReflection

        private FieldInfo _scoringForNoteStartedEventInfo;
        private FieldInfo _scoreDidChangeEventInfo;
        private FieldInfo _multiplierDidChangeEventInfo;

        private void Reflect()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            _scoringForNoteStartedEventInfo = typeof(ScoreController).GetField("scoringForNoteStartedEvent", bindingFlags);
            _scoreDidChangeEventInfo = typeof(ScoreController).GetField("scoreDidChangeEvent", bindingFlags);
            _multiplierDidChangeEventInfo = typeof(ScoreController).GetField("multiplierDidChangeEvent", bindingFlags);
        }

        private void InvokeScoringStarted(ScoringElement element)
        {
            ((Delegate)_scoringForNoteStartedEventInfo?.GetValue(this))?.DynamicInvoke(new object[] { element });
        }
        private void InvokeScoreDidChange(int multiplied, int modified)
        {
            ((Delegate)_scoreDidChangeEventInfo?.GetValue(this))?.DynamicInvoke(new object[] { multiplied, modified } );
        }
        private void InvokeMultiplierDidChange(int multiplier, float progress)
        {
            ((Delegate)_multiplierDidChangeEventInfo.GetValue(this))?.DynamicInvoke(new object[] { multiplier, progress });
        }

        #endregion
    }
}
