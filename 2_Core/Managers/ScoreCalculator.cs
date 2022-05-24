using System;
using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal class ScoreCalculator {
        #region CalculateScoreFromReplay

        public static int CalculateScoreFromReplay(Replay replay) {
            var scoreCalculator = new ScoreCalculator();
            scoreCalculator.ProcessReplay(replay);
            return scoreCalculator.TotalScore;
        }

        #endregion

        #region Init

        private readonly MultiplierCounter _multiplierCounter = new();
        public int TotalScore { get; private set; }

        private ScoreCalculator() { }

        #endregion

        #region ProcessReplay

        private void ProcessReplay(Replay replay) {
            var nextNoteIndex = 0;
            var nextWallIndex = 0;

            do {
                var hasNextNote = nextNoteIndex < replay.notes.Count;
                var hasNextWall = nextWallIndex < replay.walls.Count;
                if (!hasNextNote && !hasNextWall) return;

                var nextNote = hasNextNote ? replay.notes[nextNoteIndex] : null;
                var nextWall = hasNextWall ? replay.walls[nextWallIndex] : null;

                var notePriority = hasNextNote ? nextNote.eventTime : float.MaxValue;
                var wallPriority = hasNextWall ? nextWall.time : float.MaxValue;

                if (notePriority < wallPriority) {
                    ProcessNoteEvent(nextNote);
                    nextNoteIndex += 1;
                } else {
                    ProcessWallEvent(nextWall);
                    nextWallIndex += 1;
                }
            } while (true);
        }

        private void ProcessWallEvent(WallEvent wallEvent) {
            _multiplierCounter.Decrease();
        }

        private void ProcessNoteEvent(NoteEvent noteEvent) {
            switch (noteEvent.eventType) {
                case NoteEventType.good:
                    ProcessGoodCut(noteEvent);
                    break;
                case NoteEventType.bad:
                case NoteEventType.miss:
                case NoteEventType.bomb:
                    ProcessBadCut(noteEvent);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessBadCut(NoteEvent noteEvent) {
            _multiplierCounter.Decrease();
        }

        private void ProcessGoodCut(NoteEvent noteEvent) {
            _multiplierCounter.Increase();
            var before = GetBeforeCutScore(noteEvent);
            var after = GetAfterCutScore(noteEvent);
            var acc = GetCutDistanceScore(noteEvent);
            var @fixed = 0;

            var scoringType = (NoteData.ScoringType)(noteEvent.noteID / 10000 - 2);
            if (scoringType == NoteData.ScoringType.BurstSliderElement) {
                @fixed = 20;
            }

            var total = acc + before + after + @fixed;
            TotalScore += total * _multiplierCounter.Multiplier;
        }

        #endregion

        #region MultiplierCounter

        private class MultiplierCounter {
            public int Multiplier { get; private set; } = 1;

            private int _multiplierIncreaseProgress;
            private int _multiplierIncreaseMaxProgress = 2;

            public void Reset() {
                Multiplier = 1;
                _multiplierIncreaseProgress = 0;
                _multiplierIncreaseMaxProgress = 2;
            }

            public void Increase() {
                if (Multiplier >= 8) return;

                if (_multiplierIncreaseProgress < _multiplierIncreaseMaxProgress) {
                    ++_multiplierIncreaseProgress;
                }

                if (_multiplierIncreaseProgress >= _multiplierIncreaseMaxProgress) {
                    Multiplier *= 2;
                    _multiplierIncreaseProgress = 0;
                    _multiplierIncreaseMaxProgress = Multiplier * 2;
                }
            }

            public void Decrease() {
                if (_multiplierIncreaseProgress > 0) {
                    _multiplierIncreaseProgress = 0;
                }

                if (Multiplier > 1) {
                    Multiplier /= 2;
                    _multiplierIncreaseMaxProgress = Multiplier * 2;
                }
            }
        }

        #endregion

        #region GetRawScores

        private static NoteData.ScoringType ToScoringType(NoteEvent noteEvent) {
            return (NoteData.ScoringType)(noteEvent.noteID / 10000 - 2);
        }

        private static int GetCutDistanceScore(NoteEvent noteEvent) {
            float cutDistanceToCenter = noteEvent.noteCutInfo.cutDistanceToCenter;

            var scoringType = ToScoringType(noteEvent);
            var scoring = _scoreDefinitions[scoringType];

            return Mathf.RoundToInt(scoring.maxCenterDistanceCutScore * (1f - Mathf.Clamp01(cutDistanceToCenter / 0.3f)));
        }

        private static int GetBeforeCutScore(NoteEvent noteEvent) {
            float beforeCutRating = noteEvent.noteCutInfo.beforeCutRating;
            var rating = Mathf.Clamp01(beforeCutRating);

            var scoringType = ToScoringType(noteEvent);
            var scoring = _scoreDefinitions[scoringType];

            return Mathf.RoundToInt(Mathf.LerpUnclamped(scoring.minBeforeCutScore, scoring.maxBeforeCutScore, rating));
        }

        private static int GetAfterCutScore(NoteEvent noteEvent) {
            float afterCutRating = noteEvent.noteCutInfo.afterCutRating;
            var rating = Mathf.Clamp01(afterCutRating);

            var scoringType = ToScoringType(noteEvent);
            var scoring = _scoreDefinitions[scoringType];

            return Mathf.RoundToInt(Mathf.LerpUnclamped(scoring.minAfterCutScore, scoring.maxAfterCutScore, rating));
        }

        #endregion

        private static readonly Dictionary<NoteData.ScoringType, ScoreModel.NoteScoreDefinition> _scoreDefinitions = new Dictionary<NoteData.ScoringType, ScoreModel.NoteScoreDefinition>{
            {
                NoteData.ScoringType.Normal,
                new ScoreModel.NoteScoreDefinition(15, 0, 70, 0, 30, 0)
            },
            {
                NoteData.ScoringType.SliderHead,
                new ScoreModel.NoteScoreDefinition(15, 0, 70, 30, 30, 0)
            },
            {
                NoteData.ScoringType.SliderTail,
                new ScoreModel.NoteScoreDefinition(15, 70, 70, 0, 30, 0)
            },
            {
                NoteData.ScoringType.BurstSliderHead,
                new ScoreModel.NoteScoreDefinition(15, 0, 70, 0, 0, 0)
            },
            {
                NoteData.ScoringType.BurstSliderElement,
                new ScoreModel.NoteScoreDefinition(0, 0, 0, 0, 0, 20)
            }
        };
    }
}