using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class ScoresOfClanResponseParser : JsonResponseParser<Paged<Score>>, IWebRequestResponseParser<ScoresTableContent> {
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

    public class ScoresOfClanRequest : PersistentSingletonWebRequestBase<ScoresOfClanRequest, ScoresTableContent, ScoresOfClanResponseParser> {
        public const int ScoresPerPage = 8;

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
        private static string ClanScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public static void SendPage(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag,
            string context,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendPage(userId, clanTag, mapHash, mapDiff, mapMode, context, page);
        }

        public static void SendPage(
            string userId,
            string clanTag,
            string mapHash,
            string mapDiff,
            string mapMode,
            string context,
            int page
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PAGE, page },
                { BLConstants.Param.PrimaryClan, "true" }
            };
            var url = string.Format(ClanScoresPageEndpoint, mapHash, mapDiff, mapMode, context, $"clan_{clanTag}", NetworkingUtils.ToHttpParams(query));

            SendRet(url, HttpMethod.Get);
        }

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
        private static string ClanScoresSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public static void SendSeek(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag,
            string context
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendSeek(userId, clanTag, mapHash, mapDiff, mapMode, context);
        }

        public static void SendSeek(
            string userId,
            string clanTag,
            string mapHash,
            string mapDiff,
            string mapMode,
            string context
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PrimaryClan, "true" }
            };
            var url = string.Format(ClanScoresSeekEndpoint, mapHash, mapDiff, mapMode, context, $"clan_{clanTag}", NetworkingUtils.ToHttpParams(query));

            SendRet(url, HttpMethod.Get);
        }
    }
}
