using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class ScoreStatsManager : MonoBehaviour {
        private Coroutine _scoreStatsTask;

        private void LoadStats(int scoreId) {
            if (_scoreStatsTask != null) {
                StopCoroutine(_scoreStatsTask);
                LeaderboardState.ScoreStatsRequest.TryNotifyCancelled();
            }

            LeaderboardState.ScoreStatsRequest.NotifyStarted();

            _scoreStatsTask = StartCoroutine(
                HttpUtils.GetData<ScoreStats>(String.Format(BLConstants.SCORE_STATS_BY_ID, scoreId),
                stats => {
                    LeaderboardState.ScoreStatsRequest.NotifyFinished(stats);
                },
                reason => {
                    Plugin.Log.Debug($"No score stats for id {scoreId} was found. Abort");
                    LeaderboardState.ScoreStatsRequest.NotifyFailed(reason);
                }));
        }
    }
}
