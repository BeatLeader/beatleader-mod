using System;
using System.Collections;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal static class LeaderboardsRequest {
        #region Single

        // /leaderboards/hash/{hash}
        private const string SingleEndpoint = BLConstants.BEATLEADER_API_URL + "/leaderboards/hash/{0}";

        public static IEnumerator SendSingleRequest(
            string mapHash,
            Action<HashLeaderboardsInfoResponse> onSuccess,
            Action<string> onFail
        ) {
            var url = string.Format(SingleEndpoint, mapHash);
            var requestDescriptor = new JsonGetRequestDescriptor<HashLeaderboardsInfoResponse>(url);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }

        #endregion

        #region Ranking

        // /leaderboards?page={pageIndex}&count={itemsPerPage}&date_from={unixTime}&type=ranking&sortBy=timestamp
        private const string RankingEndpoint = BLConstants.BEATLEADER_API_URL + "/leaderboards?page={0}&count={1}&date_from={2}&type=ranking&sortBy=timestamp";

        public static IEnumerator SendRankingRequest(
            long unixDateFrom,
            int page,
            int itemsPerPage,
            Action<Paged<MassLeaderboardsInfoResponse>> onSuccess,
            Action<string> onFail
        ) {
            var url = string.Format(RankingEndpoint, page, itemsPerPage, unixDateFrom);
            var requestDescriptor = new JsonGetRequestDescriptor<Paged<MassLeaderboardsInfoResponse>>(url);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }

        #endregion
    }
}