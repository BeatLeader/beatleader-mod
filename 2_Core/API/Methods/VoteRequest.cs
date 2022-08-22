using System.Collections.Generic;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class VoteRequest : PersistentSingletonRequestHandler<VoteRequest, VoteStatus> {
        // /vote/{hash}/{diff}/{mode}?rankability={rankability}&stars={stars}&type={type}
        private const string Endpoint = BLConstants.BEATLEADER_API_URL + "/vote/{0}/{1}/{2}?{3}";

        public static void SendRequest(
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

            var url = string.Format(Endpoint, mapHash, mapDiff, mapMode, HttpUtils.ToHttpParams(query));
            var requestDescriptor = new JsonPostRequestDescriptor<VoteStatus>(url);
            instance.Send(requestDescriptor);
        }
    }
}