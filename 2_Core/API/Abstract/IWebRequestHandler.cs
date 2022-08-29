using UnityEngine.Networking;

namespace BeatLeader.API {
    internal interface IWebRequestHandler<in T> {
        public void OnRequestStarted();
        public void OnRequestFinished(T result);
        public void OnRequestFailed(string reason);
        
        public void OnProgress(float uploadProgress, float downloadProgress, float overallProgress);
    }
}