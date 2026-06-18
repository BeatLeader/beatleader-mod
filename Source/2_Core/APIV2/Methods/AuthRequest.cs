using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {
    internal class AuthRequest : PersistentWebRequestBase {
        public static IWebRequest<object> Send(
            string authToken,
            string provider
        ) {
            return Send(BLConstants.SIGNIN_WITH_TICKET, HttpMethod.Post,
                () => new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("ticket", authToken),
                    new KeyValuePair<string, string>("provider", provider),
                    new KeyValuePair<string, string>("returnUrl", "/")
                }),
                new WebRequestParams {
                    RetryCount = 3
                }, waitForLogin: false);
        }
    }
}