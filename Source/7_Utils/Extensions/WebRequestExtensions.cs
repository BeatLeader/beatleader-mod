using BeatLeader.WebRequests;

namespace BeatLeader.Utils {
    public static class WebRequestExtensions {
        public static T WithProgressListener<T>(this T webRequest, WebRequestProgressChangedDelegate<T> progressListener) where T : IWebRequest {
            webRequest.ProgressChangedEvent += progressListener as WebRequestProgressChangedDelegate<IWebRequest>;
            return webRequest;
        }

        public static T WithStateListener<T>(this T webRequest, WebRequestStateChangedDelegate<T> stateListener) where T : IWebRequest {
            webRequest.StateChangedEvent += stateListener as WebRequestStateChangedDelegate<IWebRequest>;
            return webRequest;
        }
    }
}