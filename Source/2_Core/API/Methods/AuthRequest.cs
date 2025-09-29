using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class AuthRequest : PersistentWebRequestBase {
        public static IWebRequest<object> Send(
            string authToken,
            string provider
        ) {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("ticket", authToken),
                new KeyValuePair<string, string>("provider", provider),
                new KeyValuePair<string, string>("returnUrl", "/")
            });
            return Send(BLConstants.SIGNIN_WITH_TICKET, HttpMethod.Post, formContent, new WebRequestParams {
                RetryCount = 3
            }, waitForLogin: false);
        }
    }
}