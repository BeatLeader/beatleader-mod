using System.Net.Http;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class RawDataRequest : PersistentWebRequestBase<byte[], RawResponseParser> {
        public static IWebRequest<byte[]> Send(string url) {
            return SendRet(url, HttpMethod.Get);
        }
    }
}