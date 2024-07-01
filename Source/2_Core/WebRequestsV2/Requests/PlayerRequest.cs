using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class PlayerRequest : PersistentWebRequestBaseWithResult<PlayerRequest, Player, JsonWebRequestResponseParser<Player>> {
        public static IWebRequest<Player> SendRequest(string playerId) {
            return SendRet(BeatLeaderConstants.BEATLEADER_API_URL + $"/player/{playerId}", HttpMethod.Get);
        }
    }
}