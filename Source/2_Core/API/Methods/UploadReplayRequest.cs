using System.IO;
using System.IO.Compression;
using System.Text;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    public class UploadReplayRequest : PersistentSingletonRequestHandler<UploadReplayRequest, Score> {
        private static string WithCookieEndpoint => BLConstants.BEATLEADER_API_URL + "/replayoculus";

        private const int UploadRetryCount = 3;
        private const int UploadTimeoutSeconds = 120;

        protected override bool KeepState => false;

        public static void SendRequest(Replay replay) {
            var requestDescriptor = new UploadWithCookieRequestDescriptor(replay);
            instance.Send(requestDescriptor, UploadRetryCount, UploadTimeoutSeconds);
        }

        #region RequestDescriptor

        private class UploadWithCookieRequestDescriptor : IWebRequestDescriptor<Score> {
            private readonly byte[] _compressedData;

            public UploadWithCookieRequestDescriptor(Replay replay) {
                _compressedData = CompressReplay(replay);
            }

            public UnityWebRequest CreateWebRequest() {
                var request = UnityWebRequest.Put(WithCookieEndpoint, _compressedData);
                request.SetRequestHeader("Content-Encoding", "gzip");
                return request;
            }

            public Score ParseResponse(UnityWebRequest request) {
                return JsonConvert.DeserializeObject<Score>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
            }

            private static byte[] CompressReplay(Replay replay) {
                MemoryStream stream = new();
                using (var compressedStream = new GZipStream(stream, CompressionLevel.Optimal)) {
                    ReplayEncoder.Encode(replay, new BinaryWriter(compressedStream, Encoding.UTF8));
                }

                return stream.ToArray();
            }
        }

        #endregion
    }
}