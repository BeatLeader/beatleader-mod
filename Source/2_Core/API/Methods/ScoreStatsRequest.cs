using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class ScoreStatsRequest : PersistentSingletonWebRequestBase<ScoreStatsRequest, ScoreStats, JsonResponseParser<ScoreStats>> {
        // score/statistic/{scoreId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/score/statistic/{0}";

        public static void Send(int scoreId) {
            var url = string.Format(Endpoint, scoreId);
            SendRet(url, HttpMethod.Get);
        }
    }
}