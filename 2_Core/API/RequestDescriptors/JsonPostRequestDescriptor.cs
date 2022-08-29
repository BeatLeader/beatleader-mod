using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.RequestDescriptors {
    internal class JsonPostRequestDescriptor<T> : IWebRequestDescriptor<T> {
        private readonly string _url;
        private readonly string _body;

        public JsonPostRequestDescriptor(string url, string body = "") {
            _url = url;
            _body = body;
        }

        public UnityWebRequest CreateWebRequest() {
            return UnityWebRequest.Post(_url, _body);
        }

        public T ParseResponse(UnityWebRequest request) {
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
        }
    }
}