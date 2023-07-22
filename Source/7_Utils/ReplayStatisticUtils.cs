using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using UnityEngine;
using Vector3 = BeatLeader.Models.Replay.Vector3;

namespace BeatLeader.Utils {
    internal static class ReplayStatisticUtils {
        //forked from https://github.com/BeatLeader/beatleader-server/blob/master/Utils/ReplayStatisticUtils.cs
        
        #region Models

        private class NoteParams {
            public NoteParams(int noteId) {
                var id = noteId;
                if (id < 100000) {
                    scoringType = (NoteData.ScoringType)(id / 10000);
                    id -= (int)scoringType * 10000;

                    lineIndex = id / 1000;
                    id -= lineIndex * 1000;

                    noteLineLayer = id / 100;
                    id -= noteLineLayer * 100;

                    colorType = id / 10;
                    cutDirection = id - colorType * 10;
                } else {
                    scoringType = (NoteData.ScoringType)(id / 10000000);
                    id -= (int)scoringType * 10000000;

                    lineIndex = id / 1000000;
                    id -= lineIndex * 1000000;

                    noteLineLayer = id / 100000;
                    id -= noteLineLayer * 100000;

                    colorType = id / 10;
                    cutDirection = id - colorType * 10;
                }
            }

            public readonly NoteData.ScoringType scoringType;
            public readonly int lineIndex;
            public readonly int noteLineLayer;
            public readonly int colorType;
            public readonly int cutDirection;
        }

        private class NoteStruct {
            public int score;
            public int id;
            public bool isBlock;
            public float time;
            public float spawnTime;
            public NoteData.ScoringType scoringType;

            public float multiplier;
            public int totalScore;
            public int maxScore;
            public float accuracy;
            public int combo;
        }

        private class MultiplierCounter {
            public int Multiplier { get; private set; } = 1;

            private int _multiplierIncreaseProgress;
            private int _multiplierIncreaseMaxProgress = 2;

            public void Increase() {
                if (Multiplier >= 8) return;

                if (_multiplierIncreaseProgress < _multiplierIncreaseMaxProgress) {
                    ++_multiplierIncreaseProgress;
                }

                if (_multiplierIncreaseProgress < _multiplierIncreaseMaxProgress) return;
                Multiplier *= 2;
                _multiplierIncreaseProgress = 0;
                _multiplierIncreaseMaxProgress = Multiplier * 2;
            }

            public void Decrease() {
                if (_multiplierIncreaseProgress > 0) {
                    _multiplierIncreaseProgress = 0;
                }

                if (Multiplier <= 1) return;
                Multiplier /= 2;
                _multiplierIncreaseMaxProgress = Multiplier * 2;
            }
        }

        #endregion

        #region Calculations

        public static ScoreStats? ComputeScoreStats(Replay replay) {
            try {
                var result = new ScoreStats();
                var firstNoteTime = replay.notes.FirstOrDefault()?.eventTime ?? 0.0f;
                var lastNoteTime = replay.notes.LastOrDefault()?.eventTime ?? 0.0f;

                var filteredPauses = replay.pauses.Where(p => p.time >= firstNoteTime && p.time <= lastNoteTime);
                result.winTracker = new WinTracker {
                    won = replay.info.failTime < 0.01,
                    endTime = replay.frames.LastOrDefault()?.time ?? 0,
                    nbOfPause = filteredPauses.Count(),
                    jumpDistance = replay.info.jumpDistance,
                    averageHeight = replay.heights.Any() ? replay.heights.Average(h => h.height) : replay.info.height,
                    averageHeadPosition = replay.frames.Count == 0 ? new() : new() {
                        x = replay.frames.Average(f => f.head.position.x),
                        y = replay.frames.Average(f => f.head.position.y),
                        z = replay.frames.Average(f => f.head.position.z),
                    }
                };

                var hitTracker = new HitTracker();
                result.hitTracker = hitTracker;

                foreach (var item in replay.notes) {
                    var param = new NoteParams(item.noteID);
                    switch (item.eventType) {
                        case NoteEventType.bad:
                            if (item.noteCutInfo.saberType == 0) {
                                hitTracker.leftBadCuts++;
                            } else {
                                hitTracker.rightBadCuts++;
                            }
                            break;
                        case NoteEventType.miss:
                            if (param.colorType == 0) {
                                hitTracker.leftMiss++;
                            } else {
                                hitTracker.rightMiss++;
                            }
                            break;
                        case NoteEventType.bomb:
                            if (param.colorType == 0) {
                                hitTracker.leftBombs++;
                            } else {
                                hitTracker.rightBombs++;
                            }
                            break;
                        case NoteEventType.good:
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                if (!CheckReplay(replay)) return null;
                var (accuracy, structs, maxCombo) = Accuracy(replay);
                result.hitTracker.maxCombo = maxCombo;
                result.winTracker.totalScore = structs.Last().totalScore;
                result.accuracyTracker = accuracy;
                result.scoreGraphTracker = ScoreGraph(structs, (int)(replay.frames.LastOrDefault()?.time ??  0));

                return result;
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to calculate score stats:\n" + ex);
                return null;
            }
        }

        private static bool CheckReplay(Replay replay) {
            var endTime = replay.notes.Count > 0 ? replay.notes.Last().eventTime : 0;

            foreach (var note in replay.notes) {
                var param = new NoteParams(note.noteID);
                if (note.noteID >= 100000 || note.noteID <= 0 || !(endTime - note.eventTime > 1)
                    || note.eventType != NoteEventType.good || param.colorType == note.noteCutInfo.saberType) continue;
                return false;
            }

            return true;
        }

        private static (AccuracyTracker, List<NoteStruct>, int) Accuracy(Replay replay) {
            var result = new AccuracyTracker {
                gridAcc = new float[12],
                leftAverageCut = new float[3],
                rightAverageCut = new float[3]
            };

            var gridCounts = new int[12];
            var leftCuts = new int[3];
            var rightCuts = new int[3];

            var allStructs = new List<NoteStruct>();
            foreach (var note in replay.notes) {
                var param = new NoteParams(note.noteID);
                var scoreValue = ScoreForNote(note, param.scoringType);

                if (scoreValue > 0) {
                    var index = param.noteLineLayer * 4 + param.lineIndex;
                    if (index is > 11 or < 0) {
                        index = 0;
                    }

                    if (param.scoringType != NoteData.ScoringType.BurstSliderElement
                        && param.scoringType != NoteData.ScoringType.BurstSliderHead) {
                        gridCounts[index]++;
                        result.gridAcc[index] += (float)scoreValue;
                    }

                    var (before, after, acc) = CutScoresForNote(note, param.scoringType);
                    if (param.colorType == 0) {
                        if (param.scoringType != NoteData.ScoringType.SliderTail && param.scoringType != NoteData.ScoringType.BurstSliderElement) {
                            result.leftAverageCut[0] += (float)before;
                            result.leftPreswing += note.noteCutInfo.beforeCutRating;
                            leftCuts[0]++;
                        }
                        if (param.scoringType != NoteData.ScoringType.BurstSliderElement
                            && param.scoringType != NoteData.ScoringType.BurstSliderHead) {
                            result.leftAverageCut[1] += (float)acc;
                            result.accLeft += (float)scoreValue;
                            result.leftTimeDependence += Math.Abs(note.noteCutInfo.cutNormal.z);
                            leftCuts[1]++;
                        }
                        if (param.scoringType != NoteData.ScoringType.SliderHead
                            && param.scoringType != NoteData.ScoringType.BurstSliderHead
                            && param.scoringType != NoteData.ScoringType.BurstSliderElement) {
                            result.leftAverageCut[2] += (float)after;
                            result.leftPostswing += note.noteCutInfo.afterCutRating;
                            leftCuts[2]++;
                        }
                    } else {
                        if (param.scoringType != NoteData.ScoringType.SliderTail && param.scoringType != NoteData.ScoringType.BurstSliderElement) {
                            result.rightAverageCut[0] += (float)before;
                            result.rightPreswing += note.noteCutInfo.beforeCutRating;
                            rightCuts[0]++;
                        }
                        if (param.scoringType != NoteData.ScoringType.BurstSliderElement
                            && param.scoringType != NoteData.ScoringType.BurstSliderHead) {
                            result.rightAverageCut[1] += (float)acc;
                            result.rightTimeDependence += Math.Abs(note.noteCutInfo.cutNormal.z);
                            result.accRight += (float)scoreValue;
                            rightCuts[1]++;
                        }
                        if (param.scoringType != NoteData.ScoringType.SliderHead
                            && param.scoringType != NoteData.ScoringType.BurstSliderHead
                            && param.scoringType != NoteData.ScoringType.BurstSliderElement) {
                            result.rightAverageCut[2] += (float)after;
                            result.rightPostswing += note.noteCutInfo.afterCutRating;
                            rightCuts[2]++;
                        }
                    }
                }

                allStructs.Add(new NoteStruct {
                    time = note.eventTime,
                    id = note.noteID,
                    isBlock = param.colorType != 2,
                    score = scoreValue,
                    scoringType = param.scoringType,
                    spawnTime = note.spawnTime
                });
            }

            allStructs.AddRange(replay.walls.Select(static wall => new NoteStruct {
                time = wall.time,
                id = wall.wallID,
                score = -5
            }));

            for (var i = 0; i < result.gridAcc.Count(); i++) {
                if (gridCounts[i] > 0) {
                    result.gridAcc[i] /= (float)gridCounts[i];
                }
            }

            if (leftCuts[0] > 0) {
                result.leftAverageCut[0] /= (float)leftCuts[0];
                result.leftPreswing /= (float)leftCuts[0];
            }

            if (leftCuts[1] > 0) {
                result.leftAverageCut[1] /= (float)leftCuts[1];

                result.accLeft /= (float)leftCuts[1];
                result.leftTimeDependence /= (float)leftCuts[1];
            }

            if (leftCuts[2] > 0) {
                result.leftAverageCut[2] /= (float)leftCuts[2];

                result.leftPostswing /= (float)leftCuts[2];
            }

            if (rightCuts[0] > 0) {
                result.rightAverageCut[0] /= (float)rightCuts[0];
                result.rightPreswing /= (float)rightCuts[0];
            }

            if (rightCuts[1] > 0) {
                result.rightAverageCut[1] /= (float)rightCuts[1];

                result.accRight /= (float)rightCuts[1];
                result.rightTimeDependence /= (float)rightCuts[1];
            }

            if (rightCuts[2] > 0) {
                result.rightAverageCut[2] /= (float)rightCuts[2];

                result.rightPostswing /= (float)rightCuts[2];
            }

            allStructs = allStructs.OrderBy(s => s.time).ToList();

            var score = 0;
            int combo = 0, maxCombo = 0;
            var maxScore = 0;
            var fcScore = 0;
            float currentFcAcc = 0;
            var maxCounter = new MultiplierCounter();
            var normalCounter = new MultiplierCounter();

            for (var i = 0; i < allStructs.Count; i++) {
                var note = allStructs[i];
                var scoreForMaxScore = 115;
                if (note.scoringType == NoteData.ScoringType.BurstSliderHead) {
                    scoreForMaxScore = 85;
                } else if (note.scoringType == NoteData.ScoringType.BurstSliderElement) {
                    scoreForMaxScore = 20;
                }

                if (note.isBlock) {
                    maxCounter.Increase();
                    maxScore += maxCounter.Multiplier * scoreForMaxScore;
                }

                var multiplier = 1;
                if (note.score < 0) {
                    normalCounter.Decrease();
                    multiplier = normalCounter.Multiplier;
                    combo = 0;
                    if (note.isBlock) {
                        fcScore += (int)Mathf.Round(maxCounter.Multiplier * scoreForMaxScore * currentFcAcc);
                    }
                } else {
                    normalCounter.Increase();
                    combo++;
                    multiplier = normalCounter.Multiplier;
                    score += multiplier * note.score;
                    fcScore += maxCounter.Multiplier * note.score;
                }

                if (combo > maxCombo) {
                    maxCombo = combo;
                }

                note.multiplier = multiplier;
                note.totalScore = score;
                note.maxScore = maxScore;
                note.combo = combo;

                note.accuracy = note.isBlock ? (float)note.totalScore / maxScore : i == 0 ? 0 : allStructs[i - 1].accuracy;
                currentFcAcc = (float)fcScore / maxScore;
            }

            return (result, allStructs, maxCombo);
        }

        private static ScoreGraphTracker ScoreGraph(List<NoteStruct> structs, int replayLength) {
            var scoreGraph = new ScoreGraphTracker {
                graph = new float[replayLength]
            };

            var structIndex = 0;

            for (var i = 0; i < replayLength; i++) {
                var cumulative = 0.0f;
                var delimiter = 0;
                while (structIndex < structs.Count() && structs[structIndex].time < i + 1) {
                    cumulative += structs[structIndex].accuracy;
                    structIndex++;
                    delimiter++;
                }
                if (delimiter > 0) {
                    scoreGraph.graph[i] = cumulative / delimiter;
                }
                if (scoreGraph.graph[i] == 0) {
                    scoreGraph.graph[i] = i == 0 ? 1.0f : scoreGraph.graph[i - 1];
                }
            }

            return scoreGraph;
        }

        private static int ScoreForNote(NoteEvent note, NoteData.ScoringType scoringType) {
            if (note.eventType != NoteEventType.good)
                return note.eventType switch {
                    NoteEventType.bad => -2,
                    NoteEventType.miss => -3,
                    NoteEventType.bomb => -4,
                    _ => -1
                };
            var (before, after, acc) = CutScoresForNote(note, scoringType);

            return before + after + acc;
        }

        private static (int, int, int) CutScoresForNote(NoteEvent note, NoteData.ScoringType scoringType) {
            var cut = note.noteCutInfo;
            double beforeCutRawScore = 0;
            if (scoringType != NoteData.ScoringType.BurstSliderElement) {
                beforeCutRawScore = scoringType == NoteData.ScoringType.SliderTail ? 70 : Mathf.Clamp(Mathf.Round(70 * cut.beforeCutRating), 0, 70);
            }
            double afterCutRawScore = 0;
            if (scoringType != NoteData.ScoringType.BurstSliderElement) {
                if (scoringType == NoteData.ScoringType.BurstSliderHead) {
                    afterCutRawScore = 0;
                } else if (scoringType == NoteData.ScoringType.SliderHead) {
                    afterCutRawScore = 30;
                } else {
                    afterCutRawScore = Mathf.Clamp(Mathf.Round(30 * cut.afterCutRating), 0, 30);
                }
            }
            double cutDistanceRawScore = 0;
            if (scoringType == NoteData.ScoringType.BurstSliderElement) {
                cutDistanceRawScore = 20;
            } else {
                double num = 1 - Mathf.Clamp01(cut.cutDistanceToCenter / 0.3f);
                cutDistanceRawScore = Math.Round(15 * num);
            }

            return ((int)beforeCutRawScore, (int)afterCutRawScore, (int)cutDistanceRawScore);
        }

        #endregion
    }
}