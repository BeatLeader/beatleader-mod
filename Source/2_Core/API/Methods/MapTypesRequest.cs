using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class MapTypesRequest : PersistentSingletonWebRequestBase<MapTypesRequest, List<MapsTypeDescription>, JsonResponseParser<List<MapsTypeDescription>>> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/mod/mapTypes";

        public static void Send() {
            SendRet(Endpoint, HttpMethod.Get);
        }
    }
}