using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class PlaylistRequest : PersistentWebRequestBase<byte[], RawResponseParser> {
        // /playlist/{playlistId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/playlist/{0}";

        public static IWebRequest<byte[]> Send(string playlistId) {
            var url = string.Format(Endpoint, playlistId);
            return SendRet(url, HttpMethod.Get);
        }
    }
}