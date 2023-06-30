using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class LatestReleasesRequest : PersistentSingletonRequestHandler<LatestReleasesRequest, LatestReleases> {
        // /mod/lastVersions
        private const string Endpoint = BLConstants.BEATLEADER_API_URL + "/mod/lastVersions";

        public static void SendRequest() {
            var requestDescriptor = new JsonGetRequestDescriptor<LatestReleases>(Endpoint);
            Instance.Send(requestDescriptor);
        }
    }
}