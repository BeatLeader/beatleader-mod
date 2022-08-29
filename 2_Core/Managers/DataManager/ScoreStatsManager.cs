using System;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class ScoreStatsManager : IInitializable, IDisposable {
        public void Initialize() {
            LeaderboardEvents.ScoreStatsRequestedEvent += LoadStats;
        }

        public void Dispose() {
            LeaderboardEvents.ScoreStatsRequestedEvent -= LoadStats;
        }

        private static void LoadStats(int scoreId) {
            ScoreStatsRequest.SendRequest(scoreId);
        }
    }
}
