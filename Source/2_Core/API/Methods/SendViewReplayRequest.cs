using BeatLeader.API.RequestHandlers;
using BeatLeader.Utils;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class SendViewReplayRequest : PersistentSingletonRequestHandler<SendViewReplayRequest, object> {
        // /watched/{scoreId}
        private const string Endpoint = BLConstants.BEATLEADER_API_URL + "/watched/{0}";

        protected override bool KeepState => false;

        public static void SendRequest(int scoreId) {
            var requestDescriptor = new SendViewReplayRequestDescriptor(scoreId);
            Instance.Send(requestDescriptor);
        }

        #region RequestDescriptor

        private class SendViewReplayRequestDescriptor : IWebRequestDescriptor<object> {
            private readonly int _scoreId;

            public SendViewReplayRequestDescriptor(int scoreId) {
                _scoreId = scoreId;
            }

            public UnityWebRequest CreateWebRequest() {
                var url = string.Format(Endpoint, _scoreId);
                return UnityWebRequest.Get(url);
            }

            public object ParseResponse(UnityWebRequest request) {
                return null; // ignores response
            }
        }

        #endregion
    }
}
