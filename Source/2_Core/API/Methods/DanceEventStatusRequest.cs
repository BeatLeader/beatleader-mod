using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API;

public class DanceEventStatusRequest : PersistentWebRequestBase<DanceEventStatus, JsonResponseParser<DanceEventStatus>> {
    public static IWebRequest<DanceEventStatus> Send(string eventId, int? scoreId = null) {
        var query = scoreId is { } id ? $"?scoreId={id}" : "";
        return SendRet($"{BLConstants.BEATLEADER_API_URL}/event/{eventId}/dance/status{query}", HttpMethod.Get);
    }
}
