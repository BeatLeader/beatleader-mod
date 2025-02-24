using System.Collections.Generic;
using System.Linq;
using BeatLeader.API.RequestHandlers;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class ClanScoresRequest : PersistentSingletonRequestHandler<ClanScoresRequest, ScoresTableContent> {
        public const int ScoresPerPage = 8;

        #region ClanScores/Page

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
        private static string ClanScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public static void SendClanScoresPageRequest(
            LeaderboardKey beatmapKey,
            string userId,
            string clanTag,
            string context,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendClanScoresPageRequest(userId, clanTag, mapHash, mapDiff, mapMode, context, page);
        }

        public static void SendClanScoresPageRequest(
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

            Instance.Send(new ScoreRequestDescriptor(url));
        }

        #endregion

        #region ClanScores/Seek

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
        private static string ClanScoresSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public static void SendClanScoresSeekRequest(
            LeaderboardKey beatmapKey,
            string userId,
            string clanTag,
            string context
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendClanScoresSeekRequest(userId, clanTag, mapHash, mapDiff, mapMode, context);
        }

        public static void SendClanScoresSeekRequest(
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

            Instance.Send(new ScoreRequestDescriptor(url));
        }

        #endregion

        #region ClanPlayers/Page

        // /v1/clan/players/{tag}/{hash}/{diff}/{mode}/page?player={playerId}&page={page}&count={count}&primaryClan=true
        private static string ClanPlayersPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clan/players/{0}/{1}/{2}/{3}/page?{4}";

        public static void SendClanPlayersPageRequest(
            LeaderboardKey beatmapKey,
            string userId,
            string clanTag,
            int page
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendClanPlayersPageRequest(userId, clanTag, mapHash, mapDiff, mapMode, page);
        }

        public static void SendClanPlayersPageRequest(
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

            Instance.Send(new PlayersRequestDescriptor(url));
        }

        #endregion

        #region ClanPlayers/Seek

        // /v1/clan/players/{tag}/{hash}/{diff}/{mode}/around?player={playerId}&count={count}&primaryClan=true
        private static string ClanPlayersSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clan/players/{0}/{1}/{2}/{3}/around?{4}";

        public static void SendClanPlayersSeekRequest(
            LeaderboardKey beatmapKey,
            string userId,
            string clanTag
        ) {
            NetworkingUtils.BeatmapKeyToUrlParams(in beatmapKey, out var mapHash, out var mapDiff, out var mapMode);
            SendClanPlayersSeekRequest(userId, clanTag, mapHash, mapDiff, mapMode);
        }

        public static void SendClanPlayersSeekRequest(
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

            Instance.Send(new PlayersRequestDescriptor(url));
        }

        #endregion

        #region Descriptors

        private class ScoreRequestDescriptor : IWebRequestDescriptor<ScoresTableContent> {
            private readonly string _url;

            public ScoreRequestDescriptor(string url) {
                _url = url;
            }

            public UnityWebRequest CreateWebRequest() {
                return UnityWebRequest.Get(_url);
            }

            public ScoresTableContent ParseResponse(UnityWebRequest request) {
                var result = JsonConvert.DeserializeObject<Paged<Score>>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
                var seekAvailable = result.selection != null && !result.data.Any(it => ProfileManager.IsCurrentPlayer(it.Player.id));
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, false, seekAvailable);
            }
        }

        private class PlayersRequestDescriptor : IWebRequestDescriptor<ScoresTableContent> {
            private readonly string _url;

            public PlayersRequestDescriptor(string url) {
                _url = url;
            }

            public UnityWebRequest CreateWebRequest() {
                return UnityWebRequest.Get(_url);
            }

            public ScoresTableContent ParseResponse(UnityWebRequest request) {
                var result = JsonConvert.DeserializeObject<Paged<ClanPlayer>>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
                var seekAvailable = result.selection != null && !result.data.Any(it => ProfileManager.IsCurrentPlayer(it.Player.id));

                foreach (var clanPlayer in result.data) {
                    if (clanPlayer.score == null) continue;
                    clanPlayer.score.originalPlayer = clanPlayer.originalPlayer;
                }
                
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, false, seekAvailable);
            }
        }

        #endregion
    }
}