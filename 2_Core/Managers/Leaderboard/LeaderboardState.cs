using BeatLeader.Models;

namespace BeatLeader {
    internal static class LeaderboardState {
        #region Requests

        public static RequestStateHandler<Player> ProfileRequest = new();
        public static RequestStateHandler<Score> UploadRequest = new();
        public static RequestStateHandler<Paged<Score>> ScoresRequest = new();
        public static RequestStateHandler<ScoreStats> ScoreStatsRequest = new();

        #endregion
    }
}