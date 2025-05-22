using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class LatestReleasesRequest : PersistentSingletonWebRequestBase<LatestReleasesRequest, LatestReleases, JsonResponseParser<LatestReleases>> {
        // /mod/lastVersions
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/mod/lastVersions";

        public static void Send() {
            SendRet(Endpoint, HttpMethod.Get);
        }
    }
}