using System.Collections.Generic;
using System;
using System.Linq;
using static ScoreMultiplierCounter;
using BeatLeader.Replays.Emulating;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Scoring
{
    public class ReplayerScoreController : MonoBehaviour, IReplayerScoreController
    {
        [Inject] protected readonly Replay _replay;
        [Inject] protected readonly SimpleNoteComparatorsSpawner _simpleNoteComparatorsSpawner;
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly SimpleCutScoringElement.Pool _simpleCutScoringElementPool;
        [Inject] protected readonly BeatmapTimeController _beatmapTimeController;
        [Inject] protected readonly IScoringInterlayer _scoringInterlayer;
        [Inject] protected readonly IReadonlyBeatmapData _beatmapData;

        protected GameplayModifiersModelSO _gameplayModifiersModel;
        protected List<GameplayModifierParamsSO> _gameplayModifierParams;

        protected int _maxComboAfterRescoring;
        protected int _comboAfterRescoring;

        public int maxComboAfterRescoring => _maxComboAfterRescoring;
        public int comboAfterRescoring => _comboAfterRescoring;

        public event Action<int, int, bool> onComboChangedAfterRescoring;

        #region BaseGame stuff
        [Inject] protected readonly GameplayModifiers _gameplayModifiers;
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly IGameEnergyCounter _gameEnergyCounter;
        [Inject] protected readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] protected readonly BadCutScoringElement.Pool _badCutScoringElementPool;
        [Inject] protected readonly MissScoringElement.Pool _missScoringElementPool;
        [Inject] protected readonly PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction;

        protected readonly ScoreMultiplierCounter _maxScoreMultiplierCounter = new ScoreMultiplierCounter();
        protected readonly ScoreMultiplierCounter _scoreMultiplierCounter = new ScoreMultiplierCounter();
        protected readonly List<float> _sortedNoteTimesWithoutScoringElements = new List<float>(50);
        protected readonly List<ScoringElement> _sortedScoringElementsWithoutMultiplier = new List<ScoringElement>(50);
        protected readonly List<ScoringElement> _scoringElementsWithMultiplier = new List<ScoringElement>(50);
        protected readonly List<ScoringElement> _scoringElementsToRemove = new List<ScoringElement>(50);

        protected int _modifiedScore;
        protected int _multipliedScore;
        protected int _immediateMaxPossibleMultipliedScore;
        protected int _immediateMaxPossibleModifiedScore;
        protected float _prevMultiplierFromModifiers;

        public int multipliedScore => _multipliedScore;
        public int modifiedScore => _modifiedScore;
        public int immediateMaxPossibleMultipliedScore => _immediateMaxPossibleMultipliedScore;
        public int immediateMaxPossibleModifiedScore => _immediateMaxPossibleModifiedScore;

        public event Action<int, int> scoreDidChangeEvent;
        public event Action<int, float> multiplierDidChangeEvent;
        public event Action<ScoringElement> scoringForNoteStartedEvent;
        public event Action<ScoringElement> scoringForNoteFinishedEvent;
        #endregion

        public virtual void Start()
        {
            _gameplayModifiersModel = Resources.FindObjectsOfTypeAll<GameplayModifiersModelSO>().First();
            _gameplayModifierParams = _gameplayModifiersModel.CreateModifierParamsList(_gameplayModifiers);
            _prevMultiplierFromModifiers = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent += HandlePlayerHeadDidEnterObstacles;
            _beatmapObjectManager.noteWasCutEvent += HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            _beatmapTimeController.onSongRewind += RescoreInTimeSpan;
        }
        public virtual void OnDestroy()
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
                _beatmapTimeController.onSongRewind -= RescoreInTimeSpan;
            }
        }
        public virtual void LateUpdate()
        {
            float num = (_sortedNoteTimesWithoutScoringElements.Count > 0) ? _sortedNoteTimesWithoutScoringElements[0] : float.MaxValue;
            float num2 = _audioTimeSyncController.songTime + 0.15f;
            int num3 = 0;
            bool flag = false;
            foreach (ScoringElement item in _sortedScoringElementsWithoutMultiplier)
            {
                if (item.time < num2 || item.time > num)
                {
                    flag |= _scoreMultiplierCounter.ProcessMultiplierEvent(item.multiplierEventType);
                    if (item.wouldBeCorrectCutBestPossibleMultiplierEventType == ScoreMultiplierCounter.MultiplierEventType.Positive)
                    {
                        _maxScoreMultiplierCounter.ProcessMultiplierEvent(ScoreMultiplierCounter.MultiplierEventType.Positive);
                    }

                    item.SetMultipliers(_scoreMultiplierCounter.multiplier, _maxScoreMultiplierCounter.multiplier);
                    _scoringElementsWithMultiplier.Add(item);
                    num3++;
                    continue;
                }

                break;
            }

            _sortedScoringElementsWithoutMultiplier.RemoveRange(0, num3);
            if (flag)
            {
                multiplierDidChangeEvent?.Invoke(_scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress);
            }

            bool flag2 = false;
            _scoringElementsToRemove.Clear();
            foreach (ScoringElement item2 in _scoringElementsWithMultiplier)
            {
                if (item2.isFinished)
                {
                    if (item2.maxPossibleCutScore > 0f)
                    {
                        flag2 = true;
                        _multipliedScore += item2.cutScore * item2.multiplier;
                        _immediateMaxPossibleMultipliedScore += item2.maxPossibleCutScore * item2.maxMultiplier;
                    }

                    _scoringElementsToRemove.Add(item2);
                    scoringForNoteFinishedEvent?.Invoke(item2);
                }
            }

            foreach (ScoringElement item3 in _scoringElementsToRemove)
            {
                DespawnScoringElement(item3);
                _scoringElementsWithMultiplier.Remove(item3);
            }

            _scoringElementsToRemove.Clear();
            float totalMultiplier = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
            if (_prevMultiplierFromModifiers != totalMultiplier)
            {
                _prevMultiplierFromModifiers = totalMultiplier;
                flag2 = true;
            }
            if (flag2)
            {
                _modifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(_multipliedScore, totalMultiplier);
                _immediateMaxPossibleModifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(_immediateMaxPossibleMultipliedScore, totalMultiplier);
                scoreDidChangeEvent?.Invoke(_multipliedScore, _modifiedScore);
            }
            //Debug.Log($"{_multipliedScore}/{_modifiedScore}");
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

            onComboChangedAfterRescoring?.Invoke(_comboAfterRescoring, _maxComboAfterRescoring, broke);
            scoreDidChangeEvent?.Invoke(_multipliedScore, _modifiedScore);
            multiplierDidChangeEvent?.Invoke(_scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress);
        }
        public virtual void HandleNoteWasSpawned(NoteController noteController)
        {
            if (noteController.noteData.scoringType != NoteData.ScoringType.Ignore)
            {
                ListExtensions.InsertIntoSortedListFromEnd(_sortedNoteTimesWithoutScoringElements, noteController.noteData.time);
            }
        }
        public virtual void HandleNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteCutInfo.noteData.scoringType != NoteData.ScoringType.Ignore)
            {
                if (noteCutInfo.allIsOK)
                {
                    ScoringData scoringData = new ScoringData();
                    SimpleNoteCutComparator comparator = null;
                    if (_simpleNoteComparatorsSpawner != null && _simpleNoteComparatorsSpawner.TryGetLoadedComparator(noteController, out comparator))
                    {
                        scoringData = new ScoringData(comparator.noteController.noteData, comparator.noteCutEvent,
                            comparator.noteController.worldRotation, comparator.noteController.inverseWorldRotation,
                            comparator.noteController.noteTransform.localRotation, comparator.noteController.noteTransform.position);
                    }
                    else
                    {
                        scoringData = new ScoringData(noteController, noteController.GetNoteEvent(_replay));
                    }

                    if (scoringData.noteEvent.noteCutInfo == null || scoringData.noteEvent.eventType == NoteEventType.miss)
                    {
                        if (!_initData.noteSyncMode)
                        {
                            Plugin.Log.Critical("I said you, don't do it... Note not handled exception!");
                        }
                        return;
                    }

                    SimpleCutScoringElement inElement = _simpleCutScoringElementPool.Spawn();
                    ScoringElement outElement = inElement;
                    inElement.Init(scoringData);
                    comparator?.HandleNoteControllerNoteWasCut();

                    outElement = _scoringInterlayer.Convert<GoodCutScoringElement>(inElement);

                    DespawnScoringElement(inElement);
                    ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, outElement);
                    scoringForNoteStartedEvent?.Invoke(outElement);
                    _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
                }
                else
                {
                    BadCutScoringElement badCutScoringElement = _badCutScoringElementPool.Spawn();
                    badCutScoringElement.Init(noteCutInfo.noteData);
                    ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, badCutScoringElement);
                    scoringForNoteStartedEvent?.Invoke(badCutScoringElement);
                    _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
                }
            }
        }
        public virtual void HandleNoteWasMissed(NoteController noteController)
        {
            NoteData noteData = noteController.noteData;
            if (noteData.scoringType != NoteData.ScoringType.Ignore)
            {
                MissScoringElement missScoringElement = _missScoringElementPool.Spawn();
                missScoringElement.Init(noteData);
                ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, missScoringElement);
                this.scoringForNoteStartedEvent?.Invoke(missScoringElement);
                _sortedNoteTimesWithoutScoringElements.Remove(noteData.time);
            }
        }
        public virtual void HandlePlayerHeadDidEnterObstacles()
        {
            if (_scoreMultiplierCounter.ProcessMultiplierEvent(ScoreMultiplierCounter.MultiplierEventType.Negative))
            {
                this.multiplierDidChangeEvent?.Invoke(_scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress);
            }
        }
        public virtual void DespawnScoringElement(ScoringElement scoringElement)
        {
            if (scoringElement != null)
            {
                if (scoringElement as GoodCutScoringElement != null) return;

                SimpleCutScoringElement simpleCutScoringElement;
                if ((simpleCutScoringElement = (scoringElement as SimpleCutScoringElement)) != null)
                {
                    SimpleCutScoringElement item2 = simpleCutScoringElement;
                    _simpleCutScoringElementPool.Despawn(item2);
                    return;
                }

                BadCutScoringElement badCutScoringElement;
                if ((badCutScoringElement = (scoringElement as BadCutScoringElement)) != null)
                {
                    BadCutScoringElement item2 = badCutScoringElement;
                    _badCutScoringElementPool.Despawn(item2);
                    return;
                }

                MissScoringElement missScoringElement;
                if ((missScoringElement = (scoringElement as MissScoringElement)) != null)
                {
                    MissScoringElement item3 = missScoringElement;
                    _missScoringElementPool.Despawn(item3);
                    return;
                }
            }

            throw new ArgumentOutOfRangeException();
        }
        public virtual void SetEnabled(bool enabled)
        {
            base.enabled = enabled;
        }
    }
}
