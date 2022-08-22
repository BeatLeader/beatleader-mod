using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.RequestDescriptors {
    internal class JsonGetRequestDescriptor<T> : IWebRequestDescriptor<T> {
        private readonly string _url;

        public JsonGetRequestDescriptor(string url) {
            _url = url;
        }

        public UnityWebRequest CreateWebRequest() {
            return UnityWebRequest.Get(_url);
        }

        public T ParseResponse(UnityWebRequest request) {
            var options = new JsonSerializerSettings() {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text, options);
        }
    }
}