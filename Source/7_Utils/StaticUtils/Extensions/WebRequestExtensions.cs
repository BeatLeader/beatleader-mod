using BeatLeader.WebRequests;

namespace BeatLeader.Utils {
    public static class WebRequestExtensions {
        public static T WithProgressListener<T>(this T webRequest, WebRequestProgressChangedDelegate progressListener) where T : IWebRequest {
            webRequest.ProgressChangedEvent += progressListener;
            return webRequest;
        }

        public static T WithStateListener<T>(this T webRequest, WebRequestStateChangedDelegate stateListener) where T : IWebRequest {
            webRequest.StateChangedEvent += stateListener;
            return webRequest;
        }
    }
}