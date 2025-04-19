using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using System.Net.Http;
using BeatLeader.WebRequests;
using System.Net.Http.Headers;

namespace BeatLeader.API {
    internal class UploadPlayRequest : PersistentSingletonWebRequestBase<Score, JsonResponseParser<Score>> {
        private static string WithCookieEndpoint => BLConstants.BEATLEADER_API_URL + "/replayoculus?{0}";

        public static void Send(Replay replay, PlayEndData data) {
            var query = new Dictionary<string, object>() {
                { "type", (int)data.EndType },
                { "time", data.Time }
            };
            var url = string.Format(WithCookieEndpoint, NetworkingUtils.ToHttpParams(query));

            var compressedData = CompressReplay(replay);
            var content = new ByteArrayContent(compressedData);

            SendRet(url, 
                HttpMethod.Put, 
                content, 
                new WebRequestParams {
                    RetryCount = 1,
                    TimeoutSeconds = 15
                }, 
                (HttpRequestHeaders headers) => {
                    headers.Add("Content-Encoding", "gzip");
                }
            );
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
