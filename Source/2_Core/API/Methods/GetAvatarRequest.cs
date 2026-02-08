using System.Net.Http;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class GetAvatarRequest : PersistentWebRequestBase<AvatarSettings, JsonResponseParser<AvatarSettings>> {
        public static IWebRequest<AvatarSettings> Send(string playerId, CancellationToken token = default) {
            return SendRet($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/ingameavatar", HttpMethod.Get, token: token);
        }
    }
}