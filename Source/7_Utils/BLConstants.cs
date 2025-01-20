using BeatLeader.Models;

namespace BeatLeader.Utils {
    public static class BLConstants {
        #region HTTP Status codes

        public const int MaintenanceStatus = 503;
        public const int OutdatedModStatus = 418;
        public const int Unauthorized = 401;

        #endregion

        #region Basic links

        public static string BEATLEADER_API_URL => PluginConfig.MainServer.GetAPIUrl();

        public static string BEATLEADER_WEBSITE_URL => PluginConfig.MainServer.GetWebsiteUrl();

        #endregion

        #region Signin

        internal static string SIGNIN_WITH_TICKET => //  /signin
            BEATLEADER_API_URL + "/signin";

        internal static string OCULUS_PC_SIGNIN => // /signin?action=oculuspc&token={user_id}
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
            public static readonly string PrimaryClan = "primaryClan";
        }
    }
}