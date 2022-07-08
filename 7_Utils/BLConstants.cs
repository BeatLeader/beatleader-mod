namespace BeatLeader.Utils {
    internal static class BLConstants {
        #region HTTP Status codes

        public const int MaintenanceStatus = 503;
        public const int OutdatedModStatus = 418;

        #endregion

        #region Basic links

        public const string REPLAY_UPLOAD_URL = "https://api.beatleader.xyz/replay";

        public const string BEATLEADER_API_URL = "https://api.beatleader.xyz";

        #endregion

        #region Voting

        public const string VOTE = // /vote/steam/{hash}/{diff}/{mode}?rankability={rankability}&stars={stars}&type={type}&ticket={ticket}
            BEATLEADER_API_URL + "/vote/steam/{0}/{1}/{2}?{3}";

        public const string VOTE_STATUS = // /votestatus/{hash}/{diff}/{mode}?player={playerId}
            BEATLEADER_API_URL + "/votestatus/{0}/{1}/{2}?player={3}";

        #endregion

        #region Notifications

        public const string LATEST_RELEASES = BEATLEADER_API_URL + "/mod/lastVersions";
        
        public const string RANKED_PLAYLIST = BEATLEADER_API_URL + "/playlist/ranked";

        #endregion

        #region Leaderboard requests

        public const string SCORES_BY_HASH_PAGED = // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
            BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public const string SCORES_BY_HASH_SEEK = // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
            BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public const int SCORE_PAGE_SIZE = 10;

        #endregion

        #region Profile
        
        public const string PROFILE_BY_ID = // /player/{user_id}
            BEATLEADER_API_URL + "/player/{0}";

        #endregion

        #region Score stats

        public const string SCORE_STATS_BY_ID = // score/statistic/{scoreId}
            BEATLEADER_API_URL + "/score/statistic/{0}";

        #endregion

        #region Modifiers

        public const string MODIFIERS_URL = BEATLEADER_API_URL + "/modifiers";

        #endregion

        internal class Param {
            public static readonly string PLAYER = "player";
            public static readonly string PAGE = "page";
            public static readonly string COUNT = "count";
        }
    }
}
