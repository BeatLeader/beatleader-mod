using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class VoteStatusRequest : PersistentSingletonRequestHandler<VoteStatusRequest, VoteStatus> {
        // /votestatus/{hash}/{diff}/{mode}?player={playerId}
        private const string Endpoint = BeatLeaderConstants.BEATLEADER_API_URL + "/votestatus/{0}/{1}/{2}?player={3}";

        public static void SendRequest(
            string mapHash,
            string mapDiff,
            string mapMode,
            string userId
        ) {
            var url = string.Format(Endpoint, mapHash, mapDiff, mapMode, userId);
            var requestDescriptor = new JsonGetRequestDescriptor<VoteStatus>(url);
            instance.Send(requestDescriptor);
        }
    }
}