using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class PlatformEventsRequest : PersistentWebRequestBase<Paged<PlatformEvent>, JsonResponseParser<Paged<PlatformEvent>>> {
        public static IWebRequest<Paged<PlatformEvent>> Send() {
            return SendRet($"{BLConstants.BEATLEADER_API_URL}/mod/events", HttpMethod.Get);
        }
    }
}