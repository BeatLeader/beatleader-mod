using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {
    public class MapTypesRequest : PersistentWebRequestBase<MapsTypeDescription[], JsonResponseParser<MapsTypeDescription[]>> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/mod/mapTypes";

        public static IWebRequest<MapsTypeDescription[]> Send() {
            return SendRet(Endpoint, HttpMethod.Get);
        }
    }
}