using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class RemoveFriendRequestCustomParser : IWebRequestResponseParser<Player> {
        private Player? _player;
        public RemoveFriendRequestCustomParser() {
        }
        public RemoveFriendRequestCustomParser(Player player) {
            _player = player;
        }

        public Player? ParseResponse(byte[] bytes) {
            return _player;
        }
    }
    public class RemoveFriendRequest : PersistentSingletonWebRequestBase<RemoveFriendRequest, Player, RemoveFriendRequestCustomParser> {
        // /user/friend?playerId={playerId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/user/friend?playerId={0}";

        public static void Send(Player player) {
            var url = string.Format(Endpoint, player.id);
            SendRet(url, HttpMethod.Delete, customParser: new RemoveFriendRequestCustomParser(player));
        }
    }
}
