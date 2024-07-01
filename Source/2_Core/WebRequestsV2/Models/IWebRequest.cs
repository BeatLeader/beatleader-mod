using System.Net;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public delegate void WebRequestStateChangedDelegate<in T>(T instance, RequestState state, string? failReason) where T : IWebRequest;

    public delegate void WebRequestProgressChangedDelegate<in T>(T instance, float downloadProgress, float uploadProgress, float overallProgress) where T : IWebRequest;

    public interface IWebRequest<TResult> : IWebRequest {
        TResult? Result { get; }
        
        new event WebRequestStateChangedDelegate<IWebRequest<TResult>>? StateChangedEvent;
        new event WebRequestProgressChangedDelegate<IWebRequest<TResult>>? ProgressChangedEvent;
        
        new Task<IWebRequest<TResult>> Join();
    }

    public interface IWebRequest {
        RequestState RequestState { get; }
        HttpStatusCode RequestStatusCode { get; }
        string? FailReason { get; }

        float DownloadProgress { get; }
        float UploadProgress { get; }
        float OverallProgress { get; }

        event WebRequestStateChangedDelegate<IWebRequest>? StateChangedEvent;
        event WebRequestProgressChangedDelegate<IWebRequest>? ProgressChangedEvent;

        Task<IWebRequest> Join();
    }
}