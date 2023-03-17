using UnityEngine.Networking;

namespace BeatLeader.API {
    public interface IWebRequestDescriptor<out T> {
        public UnityWebRequest CreateWebRequest();
        public T ParseResponse(UnityWebRequest request);
    }
}