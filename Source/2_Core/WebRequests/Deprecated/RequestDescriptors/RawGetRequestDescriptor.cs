using UnityEngine.Networking;

namespace BeatLeader.API.RequestDescriptors {
    internal class RawGetRequestDescriptor : IWebRequestDescriptor<byte[]> {
        private readonly string _url;

        public RawGetRequestDescriptor(string url) {
            _url = url;
        }

        public UnityWebRequest CreateWebRequest() {
            return UnityWebRequest.Get(_url);
        }

        public byte[] ParseResponse(UnityWebRequest request) {
            return request.downloadHandler.data;
        }
    }
}