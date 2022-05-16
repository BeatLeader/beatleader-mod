namespace BeatLeader.Utils {
    internal static class BLConstants {
        #region HTTP Status codes

        public const int MaintenanceStatus = 503;
        public const int OutdatedModStatus = 418;

        #endregion

        #region Basic links

        public static readonly string REPLAY_UPLOAD_URL = "https://api.beatleader.xyz/replay";

        public static readonly string BEATLEADER_API_URL = "https://api.beatleader.xyz";

        #endregion

        #region Leaderboard requests

        public static readonly string SCORES_BY_HASH_PAGED = // /v2/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
            BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public static readonly string SCORES_BY_HASH_SEEK = // /v2/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
            BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public static readonly int SCORE_PAGE_SIZE = 10;

        #endregion

        #region Profile
        public static readonly string PROFILE_BY_ID = // /player/{user_id}
            BEATLEADER_API_URL + "/player/{0}";

        #endregion

        #region Score stats

        public static readonly string SCORE_STATS_BY_ID = // score/statistic/{scoreId}
            BEATLEADER_API_URL + "/score/statistic/{0}";

        #endregion

        #region Modifiers

        public static readonly string MODIFIERS_URL = BEATLEADER_API_URL + "/modifiers";

        #endregion

        internal class Param {
            public static readonly string PLAYER = "player";
            public static readonly string PAGE = "page";
            public static readonly string COUNT = "count";
        }
    }
}
