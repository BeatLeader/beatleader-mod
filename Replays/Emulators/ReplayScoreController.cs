using BeatLeader.UI;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Emulators
{
    public class ReplayScoreController : ScoreController
    {
        [Inject] protected readonly ReplayCutScoringElement.Pool _replayCutScoringElementPool;

        public new event Action<int, int> scoreDidChangeEvent;
        public new event Action<int, float> multiplierDidChangeEvent;
        public event Action<ScoringElement> scoringForNoteFinishedEvent;

        public override void Start()
        {
            _gameplayModifiersModel = Resources.FindObjectsOfTypeAll<GameplayModifiersModelSO>().First();
            _gameplayModifierParams = _gameplayModifiersModel.CreateModifierParamsList(_gameplayModifiers);
            _prevMultiplierFromModifiers = _gameplayModifiersModel.GetTotalMultiplier(_gameplayModifierParams, _gameEnergyCounter.energy);
            _playerHeadAndObstacleInteraction.headDidEnterObstaclesEvent += HandlePlayerHeadDidEnterObstacles;
            _beatmapObjectManager.noteWasCutEvent += HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            //ВОТ И КАКОГО ХУЯ ТУТ БЫЛА CS0229? поэтому пришлось скрыть все ивенты
            scoreDidChangeEvent += (int modScore, int mulScore) => Debug.LogWarning($"{modScore} - {mulScore}");
        }
        public override void LateUpdate()
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
                this.multiplierDidChangeEvent?.Invoke(_scoreMultiplierCounter.multiplier, _scoreMultiplierCounter.normalizedProgress);
            }

            bool flag2 = false;
            _scoringElementsToRemove.Clear();
            foreach (ScoringElement item2 in _scoringElementsWithMultiplier)
            {
                if (item2.isFinished)
                {
                    if ((float)item2.maxPossibleCutScore > 0f)
                    {
                        flag2 = true;
                        _multipliedScore += item2.cutScore * item2.multiplier;
                        _immediateMaxPossibleMultipliedScore += item2.maxPossibleCutScore * item2.maxMultiplier;
                    }

                    _scoringElementsToRemove.Add(item2);
                    this.scoringForNoteFinishedEvent?.Invoke(item2);
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
                this.scoreDidChangeEvent?.Invoke(_multipliedScore, _modifiedScore);
            }
        }
        public override void HandleNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteCutInfo.noteData.scoringType != NoteData.ScoringType.Ignore)
            {
                if (noteCutInfo.allIsOK)
                {
                    ReplayCutScoringElement replayCutScoringElement = _replayCutScoringElementPool.Spawn();
                    replayCutScoringElement.Init(noteCutInfo, ReplayMenuUI.replayData);
                    _sortedScoringElementsWithoutMultiplier.InsertIntoSortedListFromEnd(replayCutScoringElement);
                    //this.scoringForNoteStartedEvent?.Invoke(replayCutScoringElement);
                    _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
                }
                else
                {
                    BadCutScoringElement badCutScoringElement = _badCutScoringElementPool.Spawn();
                    badCutScoringElement.Init(noteCutInfo.noteData);
                    ListExtensions.InsertIntoSortedListFromEnd(_sortedScoringElementsWithoutMultiplier, badCutScoringElement);
                    //this.scoringForNoteStartedEvent?.Invoke(badCutScoringElement);
                    _sortedNoteTimesWithoutScoringElements.Remove(noteCutInfo.noteData.time);
                }
            }
        }
        public override void DespawnScoringElement(ScoringElement scoringElement)
        {
            base.DespawnScoringElement(scoringElement);
            if (scoringElement != null)
            {
                ReplayCutScoringElement replayCutScoringElement;
                if ((replayCutScoringElement = scoringElement as ReplayCutScoringElement) != null)
                {
                    ReplayCutScoringElement item = replayCutScoringElement;
                    _replayCutScoringElementPool.Despawn(item);
                    return;
                }
            }
        }
    }
}
