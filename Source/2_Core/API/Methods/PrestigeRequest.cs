using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {

    internal class PrestigeRequest : PersistentSingletonWebRequestBase<PrestigeRequest, User, JsonResponseParser<User>> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/experience/prestige";

        public static void Send() {
            var url = string.Format(Endpoint);
            SendRet(url, HttpMethod.Get);
        }
    }
}