using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Replay;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class ReplayResponseParser : IWebRequestResponseParser<Replay> {
        public Replay? ParseResponse(byte[] bytes) {
            return ReplayDecoder.DecodeReplay(bytes);
        }
    }

    public class DownloadReplayRequest : PersistentWebRequestBase<Replay, ReplayResponseParser> {
        public static IWebRequest<Replay> SendRequest(string url, CancellationToken token = default) {
            return SendRet(url, HttpMethod.Get, token: token);
        }
    }

    public class StaticReplayRequest : PersistentSingletonWebRequestBase<StaticReplayRequest, Replay, ReplayResponseParser> {
        public static void Send(string url) {
            SendRet(url, HttpMethod.Get);
        }
    }
}