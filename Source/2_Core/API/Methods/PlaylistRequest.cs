using System;
using System.Collections;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal static class PlaylistRequest {
        // /playlist/{playlistId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/playlist/{0}";

        public static IEnumerator SendRequest(string playlistId, Action<byte[]> onSuccess, Action<string> onFail) {
            var url = string.Format(Endpoint, playlistId);
            var requestDescriptor = new RawGetRequestDescriptor(url);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }
    }
}