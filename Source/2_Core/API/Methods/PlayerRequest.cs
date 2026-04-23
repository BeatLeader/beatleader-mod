using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {
    public class PlayerRequest : PersistentWebRequestBase<Player, JsonResponseParser<Player>> {
        public static IWebRequest<Player> SendRequest(string playerId) {
            return SendRet(BLConstants.BEATLEADER_API_URL + $"/player/{playerId}", HttpMethod.Get);
        }
    }
}