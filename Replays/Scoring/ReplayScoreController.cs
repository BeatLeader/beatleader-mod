using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Zenject;
using BeatLeader.Replays.MapEmitating;
using BeatLeader.Models;
using BeatLeader.Replays.Scoring;
using BeatLeader.Utils;

namespace BeatLeader.Replays.Scoring
{
    public class ReplayScoreController : MonoBehaviour, IScoreController
    {
        [Inject] protected readonly Replay _replay;
        [Inject] protected readonly SimpleNoteComparatorsSpawner _simpleNoteComparatorsSpawner;
        [Inject] protected readonly ReplayManualInstaller.InitData _initData;
        [Inject] protected readonly SimpleScoringInterlayer.Pool _interlayerPool;
        [Inject] protected readonly SimpleCutScoringElement.Pool _simpleCutScoringElementPool;

        protected bool _compatibilityMode;

        #region BaseGame stuff
        [Inject] protected readonly GameplayModifiers _gameplayModifiers;
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly IGameEnergyCounter _gameEnergyCounter;
        [Inject] protected readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] protected readonly BadCutScoringElement.Pool _badCutScoringElementPool;
        [Inject] protected readonly MissScoringElement.Pool _missScoringElementPool;
        [Inject] protected readonly PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction;
        protected GameplayModifiersModelSO _gameplayModifiersModel;
        protected List<GameplayModifierParamsSO> _gameplayModifierParams;

        protected readonly ScoreMultiplierCounter _maxScoreMultiplierCounter = new ScoreMultiplierCounter();
        protected readonly ScoreMultiplierCounter _scoreMultiplierCounter = new ScoreMultiplierCounter();
        protected readonly List<float> _sortedNoteTimesWithoutScoringElements = new List<float>(50);
        protected readonly List<ScoringElement> _sortedScoringElementsWithoutMultiplier = new List<ScoringElement>(50);
        protected readonly List<ScoringElement> _scoringElementsWithMultiplier = new List<ScoringElement>(50);
        protected readonly List<ScoringElement> _scoringElementsToRemove = new List<ScoringElement>(50);

        public int multipliedScore => _multipliedScore;
        public int modifiedScore => _modifiedScore;
        public int immediateMaxPossibleMultipliedScore => _immediateMaxPossibleMultipliedScore;
        public int immediateMaxPossibleModifiedScore => _immediateMaxPossibleModifiedScore;

        protected int _modifiedScore;
        protected int _multipliedScore;
        protected int _immediateMaxPossibleMultipliedScore;
        protected int _immediateMaxPossibleModifiedScore;
        protected float _prevMultiplierFromModifiers;

        public event Action<int, int> scoreDidChangeEvent;
        public event Action<int, float> multiplierDidChangeEvent;
        public event Action<ScoringElement> scoringForNoteStartedEvent;
        public event Action<ScoringElement> scoringForNoteFinishedEvent;
        #endregion

        public virtual void SetEnabled(bool enabled)
        {
            base.enabled = enabled;
        }
        public virtual void Start()
        {
            _gameplayModifiersModel = Resources.FindObjectsOfTypeAll<GameplayModifiersModelSO>().First();
            _gameplayModifierParams = _gameplayModifiersModel.CreateModifierParamsList(_gameplayModifiers);
            _prevMultiplierFromModifiers = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent += HandlePlayerHeadDidEnterObstacles;
            _beatmapObjectManager.noteWasCutEvent += HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
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
                    SimpleScoringData scoringData = new SimpleScoringData();
                    SimpleNoteCutComparator comparator = null;
                    if (_simpleNoteComparatorsSpawner != null && _simpleNoteComparatorsSpawner.TryGetLoadedComparator(noteController, out comparator))
                    {
                        scoringData = new SimpleScoringData(comparator.noteController.noteData, comparator.noteCutEvent,
                            comparator.noteController.worldRotation, comparator.noteController.inverseWorldRotation,
                            comparator.noteController.noteTransform.localRotation, comparator.noteController.noteTransform.position);
                    }
                    else
                    {
                        scoringData = new SimpleScoringData(noteController, noteController.GetNoteEvent(_replay));
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
                        SimpleScoringInterlayer interlayer = _interlayerPool.Spawn();
                        interlayer.Init(inElement);
                        outElement = interlayer.scoringElement;
                        _interlayerPool.Despawn(interlayer);
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
    }
}
