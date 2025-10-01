using System.Collections.Generic;
using System.Linq;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using System.Net.Http;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class ScoreResponseParser : JsonResponseParser<Paged<Score>>, IWebRequestResponseParser<ScoresTableContent> {
        public new ScoresTableContent? ParseResponse(byte[] bytes) {
            var result = base.ParseResponse(bytes);
            if (result != null) {
                var seekAvailable = result.selection != null && !result.data.Any(it => ProfileManager.IsCurrentPlayer(it.Player.id));
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, false, seekAvailable);
            } else {
                return null;
            }
        }
    }

    public class ScoresRequest : PersistentSingletonWebRequestBase<ScoresRequest, ScoresTableContent, ScoreResponseParser> {
        private const int ScoresPerPage = 10;

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
        private static string PlayerScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public static void SendPage(
            BeatmapKey beatmapKey,
            string userId,
            string context,
            string scope,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendPage(userId, mapHash, mapDiff, mapMode, context, scope, page);
        }
        public static void SendPage(string userId,
                string mapHash,
                string mapDiff,
                string mapMode,
                string context,
                string scope,
                int page) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PAGE, page }
            };
            var url = string.Format(PlayerScoresPageEndpoint, mapHash, mapDiff, mapMode, context, scope, NetworkingUtils.ToHttpParams(query));
            SendRet(url, HttpMethod.Get);
        }

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
        private static string PlayerScoresSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public static void SendSeek(
            BeatmapKey beatmapKey,
            string userId,
            string context,
            string scope
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendSeek(userId, mapHash, mapDiff, mapMode, context, scope);
        }

        public static void SendSeek(
            string userId,
            string mapHash,
            string mapDiff,
            string mapMode,
            string context,
            string scope
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage }
            };
            var url = string.Format(PlayerScoresSeekEndpoint, mapHash, mapDiff, mapMode, context, scope, NetworkingUtils.ToHttpParams(query));

            SendRet(url, HttpMethod.Get);
        }
    }
}