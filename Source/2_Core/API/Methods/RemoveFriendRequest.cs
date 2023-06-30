using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class RemoveFriendRequest : PersistentSingletonRequestHandler<RemoveFriendRequest, Player> {
        // /user/friend?playerId={playerId}
        private const string Endpoint = BLConstants.BEATLEADER_API_URL + "/user/friend?playerId={0}";
        protected override bool KeepState => false;

        public static void SendRequest(Player player) {
            var requestDescriptor = new RequestDescriptor(player);
            Instance.Send(requestDescriptor);
        }

        private class RequestDescriptor : IWebRequestDescriptor<Player> {
            private readonly Player _player;

            public RequestDescriptor(Player player) {
                _player = player;
            }

            public UnityWebRequest CreateWebRequest() {
                var url = string.Format(Endpoint, _player.id);
                return UnityWebRequest.Delete(url);
            }

            public Player ParseResponse(UnityWebRequest request) {
                return _player;
            }
        }
    }
}