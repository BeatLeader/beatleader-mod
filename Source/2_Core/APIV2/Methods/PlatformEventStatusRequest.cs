using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2;

public class PlatformEventStatusRequest : PersistentWebRequestBase<PlatformEventStatus, JsonResponseParser<PlatformEventStatus>> {
    public static IWebRequest<PlatformEventStatus> Send(string id) {
        return SendRet($"{BLConstants.BEATLEADER_API_URL}/event/motd/{id}/status", HttpMethod.Get);
    }
}