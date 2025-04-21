using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class PlatformEventsRequest : PersistentSingletonWebRequestBase<Paged<PlatformEvent>, JsonResponseParser<Paged<PlatformEvent>>> {
        public static void Send() {
            SendRet($"{BLConstants.BEATLEADER_API_URL}/mod/events", HttpMethod.Get);
        }
    }
}