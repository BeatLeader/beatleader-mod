using System.Net.Http;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class RawDataRequest : PersistentWebRequestBaseWithResult<RawDataRequest, byte[], RawWebRequestResponseParser> {
        public static IWebRequest<byte[]> SendRequest(string url) {
            return SendRet(url, HttpMethod.Get);
        }
    }
}