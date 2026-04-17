using System.Net.Http;
using System.Threading;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class RawDataRequest : PersistentWebRequestBase<byte[], RawResponseParser> {
        private static readonly WebRequestParams RequestParams = new() {
            ResponseCompletionOption = HttpCompletionOption.ResponseContentRead
        };

        public static IWebRequest<byte[]> Send(string url, CancellationToken token = default) {
            return SendRet(url, HttpMethod.Get, requestParams: RequestParams, token: token);
        }
    }
}
