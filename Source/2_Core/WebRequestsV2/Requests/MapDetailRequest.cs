using System.Net.Http;
using BeatLeader.Models.BeatSaver;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class MapDetailRequest : PersistentWebRequestBaseWithResult<MapDetailRequest, MapDetail, JsonWebRequestResponseParser<MapDetail>> {
        public static IWebRequest<MapDetail> SendRequest(string mapHash) {
            return SendRet(BeatSaverUtils.CreateMapUrl(mapHash), HttpMethod.Get);
        }
    }
}