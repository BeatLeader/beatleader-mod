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
using System.Threading.Tasks;

namespace BeatLeader.API {
    public class UploadReplayRequest : PersistentSingletonWebRequestBase<Score, JsonResponseParser<Score>> {
        private static string WithCookieEndpoint => BLConstants.BEATLEADER_API_URL + "/replayoculus";

        public static void Send(Replay replay) {

            Task.Run(() => {
                var compressedData = CompressReplay(replay);
                var content = new ByteArrayContent(compressedData);

                SendRet(WithCookieEndpoint, HttpMethod.Put, content, 
                    new WebRequestParams {
                        RetryCount = 3,
                        TimeoutSeconds = 120
                    }, (HttpRequestHeaders headers) => {
                    headers.Add("Content-Encoding", "gzip");
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