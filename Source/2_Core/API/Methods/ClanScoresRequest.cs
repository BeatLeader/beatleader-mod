using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Newtonsoft.Json;

namespace BeatLeader.API {
    public class ClanScoreResponseParser : IWebRequestResponseParser<ScoresTableContent> {
        public async virtual Task<ScoresTableContent?> ParseResponse(byte[] bytes) {
            var result = JsonConvert.DeserializeObject<Paged<ClanScore>>(Encoding.UTF8.GetString(bytes), NetworkingUtils.SerializerSettings);
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

    public class PageClanScoreResponseParser : IWebRequestResponseParser<ScoresTableContent> {
        public async virtual Task<ScoresTableContent?> ParseResponse(byte[] bytes) {
            var result = JsonConvert.DeserializeObject<Paged<Score>>(Encoding.UTF8.GetString(bytes), NetworkingUtils.SerializerSettings);
            if (result != null) {
                var seekAvailable = result.selection != null && !result.data.Any(it => ProfileManager.IsCurrentPlayer(it.Player.id));
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, false, seekAvailable);
            } else {
                return null;
            }
        }
    }

    public class PageClanScoresRequest : PersistentSingletonWebRequestBase<ScoresTableContent, PageClanScoreResponseParser> {
        public const int ScoresPerPage = 8;

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
        private static string ClanScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public static void Send(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag,
            string context,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            Send(userId, clanTag, mapHash, mapDiff, mapMode, context, page);
        }

        public static void Send(
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
    }

    public class PageSeekClanScoresRequest : PersistentSingletonWebRequestBase<ScoresTableContent, PageClanScoreResponseParser> {
        public const int ScoresPerPage = 8;

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
        private static string ClanScoresSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public static void Send(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag,
            string context
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            Send(userId, clanTag, mapHash, mapDiff, mapMode, context);
        }

        public static void Send(
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
        
    public class ClanPlayersResponseParser : IWebRequestResponseParser<ScoresTableContent> {
        public async virtual Task<ScoresTableContent?> ParseResponse(byte[] bytes) {
            var result = JsonConvert.DeserializeObject<Paged<ClanPlayer>>(Encoding.UTF8.GetString(bytes), NetworkingUtils.SerializerSettings);
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

        public static void Send(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            Send(userId, clanTag, mapHash, mapDiff, mapMode, page);
        }

        public static void Send(
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
    }
    
    public class ClanPlayersSeekRequest : PersistentSingletonWebRequestBase<ScoresTableContent, ClanPlayersResponseParser> {
        public const int ScoresPerPage = 8;

        // /v1/clan/players/{tag}/{hash}/{diff}/{mode}/around?player={playerId}&count={count}&primaryClan=true
        private static string ClanPlayersSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clan/players/{0}/{1}/{2}/{3}/around?{4}";

        public static void Send(
            BeatmapKey beatmapKey,
            string userId,
            string clanTag
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            Send(userId, clanTag, mapHash, mapDiff, mapMode);
        }

        public static void Send(
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