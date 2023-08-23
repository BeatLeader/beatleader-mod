using System.Net;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public delegate void WebRequestStateChangedDelegate(IWebRequest instance, RequestState state, string? failReason);

    public delegate void WebRequestProgressChangedDelegate(IWebRequest instance, float downloadProgress, float uploadProgress, float overallProgress);

    public interface IWebRequest<out TResult> : IWebRequest {
        TResult? Result { get; }
    }

    public interface IWebRequest {
        RequestState RequestState { get; }
        HttpStatusCode RequestStatusCode { get; }
        string? FailReason { get; }

        float DownloadProgress { get; }
        float UploadProgress { get; }
        float OverallProgress { get; }

        event WebRequestStateChangedDelegate? StateChangedEvent;
        event WebRequestProgressChangedDelegate? ProgressChangedEvent;

        Task<IWebRequest> Join();
    }
}