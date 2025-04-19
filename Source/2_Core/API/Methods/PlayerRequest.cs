using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class PlayerRequest : PersistentWebRequestBase<Player, JsonResponseParser<Player>> {
        public static IWebRequest<Player> SendRequest(string playerId) {
            return SendRet(BLConstants.BEATLEADER_API_URL + $"/player/{playerId}", HttpMethod.Get);
        }
    }

    public class AddFriendRequest : PersistentSingletonWebRequestBase<Player, JsonResponseParser<Player>> {
        // /user/friend?playerId={playerId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/user/friend?playerId={0}";

        public static void Send(Player player) {
            var url = string.Format(Endpoint, player.id);
            SendRet(url, HttpMethod.Post);
        }
    }

    public class RemoveFriendRequest : PersistentSingletonWebRequestBase<Player, JsonResponseParser<Player>> {
        // /user/friend?playerId={playerId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/user/friend?playerId={0}";

        public static void Send(Player player) {
            var url = string.Format(Endpoint, player.id);
            SendRet(url, HttpMethod.Delete);
        }
    }
}