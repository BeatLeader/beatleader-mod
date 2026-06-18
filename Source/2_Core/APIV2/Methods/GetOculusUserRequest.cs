using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {

    internal class GetOculusUserRequest : PersistentSingletonWebRequestBase<GetOculusUserRequest, OculusUserInfo, JsonResponseParser<OculusUserInfo>> {
        // /oculususer
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/oculususer";

        public static async Task Send() {
            var authToken = await Authentication.PlatformTicket();
                
            if (authToken == null) {
                Instance_StateChangedEvent(null, WebRequests.RequestState.Failed, "Authentication failed");
                return;
            }

            SendRet(Endpoint, HttpMethod.Post,
                () => new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", authToken)
                }));
        }
    }
}