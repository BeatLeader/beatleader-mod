using System.Collections.Generic;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class ScoresRequest : PersistentSingletonRequestHandler<ScoresRequest, Paged<Score>> {
        #region Page
        
        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/page?player={playerId}&page={page}&count={count}
        private const string PageEndpoint = BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/page?{5}";
        
        public static void SendPageRequest(
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
                { BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE },
                { BLConstants.Param.PAGE, page }
            };
            var url = string.Format(PageEndpoint, mapHash, mapDiff, mapMode, context, scope, NetworkingUtils.ToHttpParams(query));
            
            var requestDescriptor = new JsonGetRequestDescriptor<Paged<Score>>(url);
            instance.Send(requestDescriptor);
        }

        #endregion

        #region Seek

        // /v3/scores/{hash}/{diff}/{mode}/{context}/{scope}/around?player={playerId}&count={count}
        public const string SeekEndpoint = BLConstants.BEATLEADER_API_URL + "/v3/scores/{0}/{1}/{2}/{3}/{4}/around?{5}";
        
        public static void SendSeekRequest(
            string userId,
            string mapHash,
            string mapDiff,
            string mapMode,
            string context,
            string scope
        ) {
            var query = new Dictionary<string, object> {
                { BLConstants.Param.PLAYER, userId },
                { BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE }
            };
            var url = string.Format(SeekEndpoint, mapHash, mapDiff, mapMode, context, scope, NetworkingUtils.ToHttpParams(query));

            var requestDescriptor = new JsonGetRequestDescriptor<Paged<Score>>(url);
            instance.Send(requestDescriptor);
        }

        #endregion
    }
}