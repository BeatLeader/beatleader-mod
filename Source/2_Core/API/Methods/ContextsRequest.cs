using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class ContextsRequest : PersistentSingletonWebRequestBase<ContextsRequest, List<ServerScoresContext>, JsonResponseParser<List<ServerScoresContext>>> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/mod/leaderboardContexts";

        public static void Send() {
            SendRet(Endpoint, HttpMethod.Get);
        }
    }
}