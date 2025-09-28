using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public delegate void WebRequestStateChangedDelegate<in T>(T instance, RequestState state, string? failReason);

    public delegate void WebRequestProgressChangedDelegate<in T>(T instance, float downloadProgress, float uploadProgress, float overallProgress);

    public interface IWebRequest<TResult> {
        TResult? Result { get; }
        
        new event WebRequestStateChangedDelegate<IWebRequest<TResult>>? StateChangedEvent;
        new event WebRequestProgressChangedDelegate<IWebRequest<TResult>>? ProgressChangedEvent;
        
        RequestState RequestState { get; }
        HttpStatusCode RequestStatusCode { get; }
        string? FailReason { get; }
        HttpContentHeaders? Headers { get; }

        float DownloadProgress { get; }
        float UploadProgress { get; }
        float OverallProgress { get; }
        
        Task<IWebRequest<TResult>> Join();
        void Cancel();
    }
}