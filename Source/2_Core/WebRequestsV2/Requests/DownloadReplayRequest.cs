using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Replay;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class DownloadReplayRequest : PersistentWebRequestBase<Replay, DownloadReplayRequest.ReplayResponseParser> {
        public static IWebRequest<Replay> SendRequest(string url, CancellationToken token = default) {
            return SendRet(url, HttpMethod.Get, token: token);
        }

        public class ReplayResponseParser : IWebRequestResponseParser<Replay> {
            public Task<Replay?> ParseResponse(byte[] bytes) {
                return Task.Run(() => ReplayDecoder.DecodeReplay(bytes))!;
            }
        }
    }
}