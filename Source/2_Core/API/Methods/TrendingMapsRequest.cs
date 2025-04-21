using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class TrendingMapsRequest : PersistentSingletonWebRequestBase<Paged<TrendingMapData>, JsonResponseParser<Paged<TrendingMapData>>> {
        public static void Send() {
            SendRet($"{BLConstants.BEATLEADER_API_URL}/mod/maps/trending", HttpMethod.Get);
        }
    }
}