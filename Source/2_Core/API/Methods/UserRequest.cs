using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {

    internal class UserRequest : PersistentSingletonWebRequestBase<User, JsonResponseParser<User>> {
        // /user
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/user";

        public static void Send() {
            var url = string.Format(Endpoint);
            SendRet(url, HttpMethod.Get);
        }
    }
}