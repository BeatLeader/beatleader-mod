using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    internal static class BLConstants
    {

        #region Basic links

        public static readonly string REPLAY_UPLOAD_URL = "https://beatleader.azurewebsites.net/replay";

        public static readonly string BEATLEADER_API_URL = "https://api.beatleader.xyz";

        #endregion

        #region Leaderboard requests

        public static readonly string GLOBAL_SCORES_BY_HASH = // /v2/scores/{hash}/{diff}/{mode}?player={playerId}&page={page}&count={count}
            BEATLEADER_API_URL + "/v2/scores/{0}/{1}/{2}?player={3}&page={4}&count={5}";

        public static readonly string COUNTRY_SCORES_BY_HASH = // /v2/scores/{hash}/{diff}/{mode}?player={playerId}&page={page}&count={count}&country={country}
            BEATLEADER_API_URL + "/v2/scores/{0}/{1}/{2}?player={3}&page={4}&count={5}&country={6}";

        public static readonly string AROUNG_ME_SCORES_BY_HASH = // /v2/scores/{hash}/{diff}/{mode}?player={playerId}&page={page}&count={count}&aroundPlayer={aroundPlayer}
            BEATLEADER_API_URL + "/v2/scores/{0}/{1}/{2}?player={3}&page={4}&count={5}&aroundPlayer={6}";

        public static readonly int SCORE_PAGE_SIZE = 10;

        #endregion

        public static readonly string PROFILE_BY_ID = // /player/{user_id}
            BEATLEADER_API_URL + "/player/{0}";
    }
}
