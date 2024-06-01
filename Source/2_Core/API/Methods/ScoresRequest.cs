using System.Collections.Generic;
using System.Linq;
using BeatLeader.API.RequestHandlers;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class ScoresRequest : PersistentSingletonRequestHandler<ScoresRequest, ScoresTableContent> {
        private const int ScoresPerPage = 10;

        #region PlayerScores/Page

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
        private static string PlayerScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";

        public static void SendPlayerScoresPageRequest(
            string userId,
            string mapHash,
            string mapDiff,
            string mapMode,
            string context,
            string scope,
            int page
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, ScoresPerPage },
                { BLConstants.Param.PAGE, page }
            };
            var url = string.Format(PlayerScoresPageEndpoint, mapHash, mapDiff, mapMode, context, scope, NetworkingUtils.ToHttpParams(query));

            instance.Send(new ScoreRequestDescriptor(url));
        }

        #endregion

        #region PlayerScores/Seek

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
        private static string PlayerScoresSeekEndpoint => BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";

        public static void SendPlayerScoresSeekRequest(
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

            instance.Send(new ScoreRequestDescriptor(url));
        }

        #endregion

        #region ClanScores/Page

        // /v1/clanScores/{hash}/{diff}/{mode}/page?page={page}&count={count}
        private static string ClanScoresPageEndpoint => BLConstants.BEATLEADER_API_URL + "/v1/clanScores/{0}/{1}/{2}/page?{3}";

        public static void SendClanScoresPageRequest(
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

            instance.Send(new ClanScoreRequestDescriptor(url));
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
                var seekAvailable = result.selection != null && !result.data.Any(it => ProfileManager.IsCurrentPlayer(it.player.id));
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, false, seekAvailable);
            }
        }

        private class ClanScoreRequestDescriptor : IWebRequestDescriptor<ScoresTableContent> {
            private readonly string _url;

            public ClanScoreRequestDescriptor(string url) {
                _url = url;
            }

            public UnityWebRequest CreateWebRequest() {
                return UnityWebRequest.Get(_url);
            }

            public ScoresTableContent ParseResponse(UnityWebRequest request) {
                var result = JsonConvert.DeserializeObject<Paged<ClanScore>>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
                return new ScoresTableContent(result.selection, result.data, result.metadata.page, result.metadata.PagesCount, true, false);
            }
        }

        #endregion
    }
}