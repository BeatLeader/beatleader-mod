using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class ClanScoreResponseParser : JsonResponseParser<Paged<ClanScore>>, IWebRequestResponseParser<ScoresTableContent> {
        public new ScoresTableContent? ParseResponse(byte[] bytes) {
            var result = base.ParseResponse(bytes);
            if (result != null) {
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, true, false);
            } else {
                return null;
            }
        }
    }

    public class ClanScoresRequest : PersistentSingletonWebRequestBase<ScoresTableContent, ClanScoreResponseParser> {
        private const int ScoresPerPage = 10;

        // /v1/clanScores/{hash}/{diff}/{mode}/page?page={page}&count={count}
        private static string ClanScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clanScores/{0}/{1}/{2}/page?{3}";

        public static void Send(
            BeatmapKey beatmapKey,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            Send(mapHash, mapDiff, mapMode, page);
        }

        public static void Send(
            string mapHash,
            string mapDiff,
            string mapMode,
            int page
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PAGE, page }
            };
            var url = string.Format(ClanScoresPageEndpoint, mapHash, mapDiff, mapMode, NetworkingUtils.ToHttpParams(query));

            SendRet(url, HttpMethod.Get);
        }
    }
}