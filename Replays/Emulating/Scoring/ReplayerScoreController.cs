using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Zenject;
using BeatLeader.Replays.Emulating;
using BeatLeader.Models;
using BeatLeader.Replays.Models;
using BeatLeader.Utils;
using static ScoreMultiplierCounter;

namespace BeatLeader.Replays.Scoring
{
    public class ReplayerScoreController : MonoBehaviour, IScoreController
    {
        [Inject] protected readonly Replay _replay;
        [Inject] protected readonly SimpleNoteComparatorsSpawner _simpleNoteComparatorsSpawner;
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly SimpleCutScoringElement.Pool _simpleCutScoringElementPool;
        [Inject] protected readonly RescoreInvoker _rescoreInvoker;
        [Inject] protected readonly IScoringInterlayer _scoringInterlayer;
        [Inject] protected readonly IReadonlyBeatmapData _beatmapData;

        protected GameplayModifiersModelSO _gameplayModifiersModel;
        protected List<GameplayModifierParamsSO> _gameplayModifierParams;
        protected bool _compatibilityMode;

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
            _rescoreInvoker.onRescoreRequested += RescoreInTimeSpan;
            _compatibilityMode = _initData.compatibilityMode;
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

            if (_rescoreInvoker != null)
            {
                _rescoreInvoker.onRescoreRequested -= RescoreInTimeSpan;
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
        public virtual void RescoreInTimeSpan(float startTime, float endTime)
        {
            List<BeatmapDataItem> filteredBeatmapItems = _beatmapData
                .GetFilteredCopy(x => x.time >= startTime && x.time <= endTime
                && x.type == BeatmapDataItem.BeatmapDataItemType.BeatmapObject ? x : null).allBeatmapDataItems.ToList();

            _modifiedScore = 0;
            _multipliedScore = 0;
            _immediateMaxPossibleModifiedScore = 0;
            _immediateMaxPossibleMultipliedScore = 0;
            _scoreMultiplierCounter.Reset();
            _maxScoreMultiplierCounter.Reset();

            int modifiedScore = 0;
            int multipliedScore = 0;
            int immediateMaxPossibleModifiedScore = 0;
            int immediateMaxPossibleMultipliedScore = 0;
            float prevMultiplierFromModifiers = 0;

            foreach (var item in filteredBeatmapItems)
            {
                NoteData noteData;
                if ((noteData = item as NoteData) != null)
                {
                    var noteEvent = noteData.GetNoteEvent(_replay);
                    if (noteEvent == null) continue;
                    int cutScore = noteEvent.ComputeNoteScore();
                    int maxScore = ScoreModel.GetNoteScoreDefinition(noteData.scoringType).maxCutScore;

                    MultiplierEventType multiplierType = noteData.ComputeNoteMultiplier();
                    _scoreMultiplierCounter.ProcessMultiplierEvent(multiplierType);
                    _maxScoreMultiplierCounter.ProcessMultiplierEvent(multiplierType == MultiplierEventType.Positive ? 
                        MultiplierEventType.Positive : MultiplierEventType.Neutral);

                    int multiplier = _scoreMultiplierCounter.multiplier;
                    int maxMultiplier = _maxScoreMultiplierCounter.multiplier;

                    if (maxScore > 0)
                    {
                        multipliedScore += cutScore * multiplier;
                        immediateMaxPossibleMultipliedScore += maxScore * maxMultiplier;
                    }

                    float totalMultiplier = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
                    if (prevMultiplierFromModifiers != totalMultiplier)
                    {
                        modifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(multipliedScore, totalMultiplier);
                        immediateMaxPossibleModifiedScore = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(immediateMaxPossibleMultipliedScore, totalMultiplier);
                    }
                    continue;
                }
                ObstacleData obstacleData;
                if ((obstacleData = item as ObstacleData) != null)
                {
                    //_scoreMultiplierCounter.ProcessMultiplierEvent(MultiplierEventType.Negative);
                    continue;
                }
            }

            _modifiedScore = modifiedScore;
            _multipliedScore = multipliedScore;
            _immediateMaxPossibleModifiedScore = immediateMaxPossibleModifiedScore;
            _immediateMaxPossibleMultipliedScore = immediateMaxPossibleMultipliedScore;
            _prevMultiplierFromModifiers = prevMultiplierFromModifiers;

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

                    if (scoringData.noteEvent.noteCutInfo == null | scoringData.noteEvent.eventType == NoteEventType.miss)
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

                    if (_compatibilityMode)
                    {
                        outElement = _scoringInterlayer.Convert(inElement, typeof(GoodCutScoringElement));
                    }

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
