using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using System.Net.Http;
using BeatLeader.WebRequests;
using System.Threading.Tasks;

namespace BeatLeader.API {
    public class UploadReplayRequest : PersistentSingletonWebRequestBase<UploadReplayRequest, ScoreUploadResponse, JsonResponseParser<ScoreUploadResponse>> {
        private static string WithCookieEndpoint => BLConstants.BEATLEADER_API_URL + "/v2/replayoculus?{0}";

        public static void Send(Replay replay, PlayEndData data) {
            Task.Run(() => {
                var query = new Dictionary<string, object>() {
                    { "time", data.Time },
                    { "type", (int)data.EndType }
                };
                var url = string.Format(WithCookieEndpoint, NetworkingUtils.ToHttpParams(query));

                var compressedData = CompressReplay(replay);
                var content = new ByteArrayContent(compressedData);
                content.Headers.ContentEncoding.Add("gzip");

                SendRet(url, HttpMethod.Put, content,
                    new WebRequestParams {
                        RetryCount = 3,
                        TimeoutSeconds = 120
                    });
            }).RunCatching();
        }

        private static byte[] CompressReplay(Replay replay) {
            MemoryStream stream = new();
            using (var compressedStream = new GZipStream(stream, CompressionLevel.Optimal)) {
                ReplayEncoder.Encode(replay, new BinaryWriter(compressedStream, Encoding.UTF8));
            }

            return stream.ToArray();
        }
    }
}