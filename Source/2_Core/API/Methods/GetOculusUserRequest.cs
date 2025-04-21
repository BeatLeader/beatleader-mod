using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {

    internal class GetOculusUserRequest : PersistentSingletonWebRequestBase<OculusUserInfo, JsonResponseParser<OculusUserInfo>> {
        // /oculususer
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/oculususer";

        public static async Task Send() {
            var authToken = await Authentication.PlatformTicket();
                
            if (authToken == null) {
                Instance_StateChangedEvent(null, WebRequests.RequestState.Failed, "Authentication failed");
                return;
            }

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", authToken)
            });
            SendRet(Endpoint, HttpMethod.Post, formContent);
        }
    }
}