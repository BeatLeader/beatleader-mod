using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    internal static class BLConstants
    {
        public static readonly string BEATLEADER_API_URL = "https://api.beatleader.xyz";

        public static readonly string GLOBAL_SCORES_BY_HASH = // /scores/{hash}/{diff}/{mode}?page={page}&count={count}
            BEATLEADER_API_URL + "/scores/{0}/{1}/{2}?page={3}&count={4}";

        public static readonly string PROFILE_BY_ID = // /player/{user_id}
            BEATLEADER_API_URL + "/player/{0}";

        public static readonly int SCORE_PAGE_SIZE = 10;
    }
}
