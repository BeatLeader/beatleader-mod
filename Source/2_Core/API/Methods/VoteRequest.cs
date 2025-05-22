using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class VoteRequest : PersistentSingletonWebRequestBase<VoteRequest, VoteStatus, JsonResponseParser<VoteStatus>> {
        // /vote/{hash}/{diff}/{mode}?rankability={rankability}&stars={stars}&type={type}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/vote/{0}/{1}/{2}?{3}";

        public static void Send(
            string mapHash,
            string mapDiff,
            string mapMode,
            Vote vote
        ) {
            var query = new Dictionary<string, object> {
                ["rankability"] = vote.Rankability
            };
            if (vote.HasStarRating) query["stars"] = vote.StarRating;
            if (vote.HasMapType) query["type"] = (int)vote.MapType;

            var url = string.Format(Endpoint, mapHash, mapDiff, mapMode, NetworkingUtils.ToHttpParams(query));
            SendRet(url, HttpMethod.Post);
        }
    }
}