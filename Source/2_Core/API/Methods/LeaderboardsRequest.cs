using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class LeaderboardRequest : PersistentSingletonWebRequestBase<LeaderboardRequest, HashLeaderboardsInfoResponse, JsonResponseParser<HashLeaderboardsInfoResponse>> {

        // /leaderboards/hash/{hash}
        private static string SingleEndpoint => BLConstants.BEATLEADER_API_URL + "/leaderboards/hash/{0}";

        public static void Send(
            string mapHash
        ) {
            var url = string.Format(SingleEndpoint, mapHash);
            SendRet(url, HttpMethod.Get);
        }
    }

    public class LeaderboardsRequest : PersistentWebRequestBase<Paged<MassLeaderboardsInfoResponse>, JsonResponseParser<Paged<MassLeaderboardsInfoResponse>>> {
        // /leaderboards?page={pageIndex}&count={itemsPerPage}&date_from={unixTime}&type=ranking&sortBy=timestamp
        private static string RankingEndpoint => BLConstants.BEATLEADER_API_URL + "/leaderboards?page={0}&count={1}&date_from={2}&type=ranking&sortBy=timestamp";

        public static IWebRequest<Paged<MassLeaderboardsInfoResponse>> Send(
            long unixDateFrom,
            int page,
            int itemsPerPage
        ) {
            var isUpdated = Plugin.Version > Hive.Versioning.Version.Parse(ConfigFileData.Instance.LastSessionModVersion);
            var url = string.Format(RankingEndpoint, page, itemsPerPage, isUpdated ? 0 : unixDateFrom);
            return SendRet(url, HttpMethod.Get);
        }
    }
}