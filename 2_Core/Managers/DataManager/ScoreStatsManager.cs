using System;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class ScoreStatsManager : MonoBehaviour {
        private Coroutine _scoreStatsTask;

        private void LoadStats(int scoreId) {
            if (_scoreStatsTask != null) {
                StopCoroutine(_scoreStatsTask);
            }

            LeaderboardEvents.ScoreStatsRequestStarted();

            _scoreStatsTask = StartCoroutine(
                HttpUtils.GetData<ScoreStats>(String.Format(BLConstants.SCORE_STATS_BY_ID, scoreId),
                stats => {
                    LeaderboardEvents.PublishStats(stats);

                },
                () => {
                    Plugin.Log.Debug($"No score stats for id {scoreId} was found. Abort");
                    LeaderboardEvents.NotifyScoreStatsFetchFailed();
                }));
        }
    }
}
