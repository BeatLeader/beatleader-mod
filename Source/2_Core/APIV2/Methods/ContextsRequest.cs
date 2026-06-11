using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {
    public class ContextsRequest : PersistentWebRequestBase<ServerScoresContext[], JsonResponseParser<ServerScoresContext[]>> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/mod/leaderboardContexts";

        public static IWebRequest<ServerScoresContext[]> Send() {
            return SendRet(Endpoint, HttpMethod.Get);
        }
    }
}