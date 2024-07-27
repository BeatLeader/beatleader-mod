using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class GetAvatarRequest : PersistentWebRequestBaseWithResult<GetAvatarRequest, AvatarSettings, JsonWebRequestResponseParser<AvatarSettings>> {
        public static IWebRequest<AvatarSettings> Send(string playerId) {
            return SendRet($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/ingameavatar", HttpMethod.Get);
        }
    }
}