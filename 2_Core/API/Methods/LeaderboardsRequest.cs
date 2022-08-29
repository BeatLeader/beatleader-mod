using System;
using System.Collections;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal static class LeaderboardsRequest {
        // /leaderboards?page={pageIndex}&count={itemsPerPage}&type={nominated/qualified/ranked}
        public const string BulkEndpoint = BLConstants.BEATLEADER_API_URL + "/leaderboards?page={0}&count={1}&type={2}";

        // /leaderboards/hash/{hash}
        public const string SingleEndpoint = BLConstants.BEATLEADER_API_URL + "/leaderboards/hash/{0}";

        public static IEnumerator SendSingleRequest(
            string mapHash,
            Action<HashLeaderboardsInfoResponse> onSuccess,
            Action<string> onFail
        ) {
            var url = string.Format(SingleEndpoint, mapHash);
            var requestDescriptor = new JsonGetRequestDescriptor<HashLeaderboardsInfoResponse>(url);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }

        public static IEnumerator SendBulkRequest(
            string type,
            int page,
            int itemsPerPage,
            Action<Paged<MassLeaderboardsInfoResponse>> onSuccess,
            Action<string> onFail
        ) {
            var url = string.Format(BulkEndpoint, page, itemsPerPage, type);
            var requestDescriptor = new JsonGetRequestDescriptor<Paged<MassLeaderboardsInfoResponse>>(url);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }
    }
}