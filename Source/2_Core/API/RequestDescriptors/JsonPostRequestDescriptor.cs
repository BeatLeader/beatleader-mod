using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace BeatLeader.API.RequestDescriptors {
    internal class JsonPostRequestDescriptor<T> : IWebRequestDescriptor<T> {
        private readonly string _url;

        private readonly string? _body;
        private readonly List<IMultipartFormSection>? _form;

        public JsonPostRequestDescriptor(string url, string body) {
            _url = url;
            _body = body;
        }

        public JsonPostRequestDescriptor(string url) {
            _url = url;
            _body = "";
        }

        public JsonPostRequestDescriptor(string url, List<IMultipartFormSection> form = null) {
            _url = url;
            _form = form;
        }

        public UnityWebRequest CreateWebRequest() {
            if (_form != null) {
                return UnityWebRequest.Post(_url, _form);
            }
            return UnityWebRequest.Post(_url, _body);
        }

        public T ParseResponse(UnityWebRequest request) {
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
        }
    }
}