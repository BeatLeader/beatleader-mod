using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class VoteStatusRequest : PersistentSingletonWebRequestBase<VoteStatus?, JsonResponseParser<VoteStatus?>> {
        // /votestatus/{hash}/{diff}/{mode}?player={playerId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/votestatus/{0}/{1}/{2}?player={3}";

        public static void SendRequest(
            string mapHash,
            string mapDiff,
            string mapMode,
            string userId
        ) {
            var url = string.Format(Endpoint, mapHash, mapDiff, mapMode, userId);
            SendRet(url, HttpMethod.Get);
        }
    }
}