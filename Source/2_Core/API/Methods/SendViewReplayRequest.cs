using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {

    public class SendViewReplayRequest : PersistentSingletonWebRequestBase<byte[], RawResponseParser> {
        // /watched/{scoreId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/watched/{0}";

        public static void Send(int scoreId) {
            var url = string.Format(Endpoint, scoreId);
            SendRet(url, HttpMethod.Get);
        }
    }
}
