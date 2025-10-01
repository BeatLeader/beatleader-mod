using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class StatsStorage {
        private static readonly Dictionary<string, (Score?, ScoreStats?)> cachedReplayStats = new();

        public static void AddStats(IReplayHeader header, Score? score, ScoreStats? stats) {
            cachedReplayStats[header.FilePath] = (score, stats);
        }

        public static bool TryGetStats(IReplayHeader header, out Score? score, out ScoreStats? stats) {
            var res = cachedReplayStats.TryGetValue(header.FilePath, out var tuple);
            score = tuple.Item1;
            stats = tuple.Item2;
            return res;
        }
    }
}