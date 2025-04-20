using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class ClanPlayersResponseParser : JsonResponseParser<Paged<ClanPlayer>>, IWebRequestResponseParser<ScoresTableContent> {
        public new ScoresTableContent? ParseResponse(byte[] bytes) {
            var result = base.ParseResponse(bytes);
            if (result != null) {
                var seekAvailable = result.selection != null && !result.data.Any(it => ProfileManager.IsCurrentPlayer(it.Player.id));

                foreach (var clanPlayer in result.data) {
                    if (clanPlayer.score == null) continue;
                    clanPlayer.score.originalPlayer = clanPlayer.originalPlayer;
                }
                
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, false, seekAvailable);
            } else {
                return null;
            }
        }
    }

    public class ClanPlayersRequest : PersistentSingletonWebRequestBase<ScoresTableContent, ClanPlayersResponseParser> {
        public const int ScoresPerPage = 8;

        // /v1/clan/players/{tag}/{hash}/{diff}/{mode}/page?player={playerId}&page={page}&count={count}&primaryClan=true
        private static string ClanPlayersPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clan/players/{0}/{1}/{2}/{3}/page?{4}";

        public static void SendPage(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendPage(userId, clanTag, mapHash, mapDiff, mapMode, page);
        }

        public static void SendPage(
            string userId,
            string clanTag,
            string mapHash,
            string mapDiff,
            string mapMode,
            int page
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PAGE, page },
                { BLConstants.Param.PrimaryClan, "true" }
            };
            var url = string.Format(ClanPlayersPageEndpoint, clanTag, mapHash, mapDiff, mapMode, NetworkingUtils.ToHttpParams(query));

            SendRet(url, HttpMethod.Get);
        }

        // /v1/clan/players/{tag}/{hash}/{diff}/{mode}/around?player={playerId}&count={count}&primaryClan=true
        private static string ClanPlayersSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clan/players/{0}/{1}/{2}/{3}/around?{4}";

        public static void SendSeek(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendSeek(userId, clanTag, mapHash, mapDiff, mapMode);
        }

        public static void SendSeek(
            string userId,
            string clanTag,
            string mapHash,
            string mapDiff,
            string mapMode
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PrimaryClan, "true" }
            };
            var url = string.Format(ClanPlayersSeekEndpoint, clanTag, mapHash, mapDiff, mapMode, NetworkingUtils.ToHttpParams(query));

            SendRet(url, HttpMethod.Get);
        }
    }
}
