using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class UploadTreeOrnamentsRequest : PersistentSingletonRequestHandler<UploadTreeOrnamentsRequest, string> {
        public static void SendRequest(ChristmasTreeOrnamentSettings[] ornaments) {
            var descriptor = new RequestDescriptor(ornaments);
            Instance.Send(descriptor);
        }

        private class RequestDescriptor : IWebRequestDescriptor<string> {
            public RequestDescriptor(ChristmasTreeOrnamentSettings[] ornaments) {
                _body = JsonConvert.SerializeObject(ornaments);
            }

            private readonly string _body;

            public UnityWebRequest CreateWebRequest() {
                return UnityWebRequest.Post($"{BLConstants.BEATLEADER_API_URL}/projecttree/ornaments", _body);
            }

            public string ParseResponse(UnityWebRequest request) {
                return "";
            }
        }
    }
}