namespace BeatLeader.Utils {
    internal static class BLConstants {
        #region HTTP Status codes

        public const int MaintenanceStatus = 503;
        public const int OutdatedModStatus = 418;
        public const int Unauthorized = 401;

        #endregion

        #region Basic links

        //public const string BEATLEADER_API_URL = "https://api.beatleader.xyz";
        public const string BEATLEADER_API_URL = "https://localhost:44313";

        //public const string BEATLEADER_WEBSITE_URL = "https://www.beatleader.xyz";
        public const string BEATLEADER_WEBSITE_URL = "http://localhost:8888";

        public const string EX_MACHINA_API_URL = "https://bs-replays-ai.azurewebsites.net";

        #endregion

        #region Signin
        
        public const string SIGNIN_WITH_TICKET = //  /signin
            BEATLEADER_API_URL + "/signin?ticket={0}";

        public const string OCULUS_PC_SIGNIN = // /signin?action=oculuspc&token={user_id}
            BEATLEADER_WEBSITE_URL + "/signin/oculuspc?token={0}";

        #endregion

        #region Website links

        public static string LeaderboardPage(string leaderboardId) {
            return $"{BEATLEADER_WEBSITE_URL}/leaderboard/global/{leaderboardId}";
        }
        
        public static string PlayerProfilePage(string playerId) {
            return $"{BEATLEADER_WEBSITE_URL}/u/{playerId}";
        }
        
        #endregion

        internal static class Param {
            public static readonly string PLAYER = "player";
            public static readonly string PAGE = "page";
            public static readonly string COUNT = "count";
        }
    }
}
