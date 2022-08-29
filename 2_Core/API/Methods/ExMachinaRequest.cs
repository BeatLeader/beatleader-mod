using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class ExMachinaRequest : PersistentSingletonRequestHandler<ExMachinaRequest, ExMachinaBasicResponse> {
        // /json/{hash}/{diffId}/basic
        private const string Endpoint = BLConstants.EX_MACHINA_API_URL + "/json/{0}/{1}/basic";

        public static void SendRequest(LeaderboardKey leaderboardKey) {
            var url = string.Format(Endpoint, leaderboardKey.Hash, leaderboardKey.DiffId);
            var requestDescriptor = new JsonGetRequestDescriptor<ExMachinaBasicResponse>(url);
            instance.Send(requestDescriptor);
        }
    }
}