using UnityEngine.Networking;

namespace BeatLeader.API {
    internal interface IWebRequestDescriptor<out T> {
        public UnityWebRequest CreateWebRequest();
        public T ParseResponse(UnityWebRequest request);
    }
}