using System.Net.Http;
using BeatLeader.Models.BeatSaver;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {
    public class MapDetailRequest : PersistentWebRequestBase<MapDetail, JsonResponseParser<MapDetail>> {
        public static IWebRequest<MapDetail> SendRequest(string mapHash) {
            return SendRet(BeatSaverUtils.CreateMapUrl(mapHash), HttpMethod.Get);
        }
    }
}