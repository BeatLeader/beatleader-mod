using UnityEngine.Networking;

namespace BeatLeader.API {
    internal interface IWebRequestHandler<in T> {
        public void OnRequestStarted();
        public void OnRequestFinished(T result);
        public void OnRequestFailed(string reason);
        
        public void OnUploadProgress(float progress);
        public void OnDownloadProgress(float progress);
        public void OnProgress(float progress);
    }
}