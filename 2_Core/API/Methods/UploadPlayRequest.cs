using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Models.Activity;
using BeatLeader.Utils;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class UploadPlayRequest : PersistentSingletonRequestHandler<UploadPlayRequest, Score> {
        private const string WithCookieEndpoint = BLConstants.BEATLEADER_API_URL + "/replayoculus?{0}";

        private const int UploadRetryCount = 1;
        private const int UploadTimeoutSeconds = 15;

        protected override bool KeepState => false;
        protected override bool AllowConcurrentRequests => true;

        public static void SendRequest(Replay replay, PlayEndData data) {
            var requestDescriptor = new UploadWithCookieRequestDescriptor(replay, data);
            instance.Send(requestDescriptor, UploadRetryCount, UploadTimeoutSeconds);
        }

        #region RequestDescriptor

        private class UploadWithCookieRequestDescriptor : IWebRequestDescriptor<Score> {
            private readonly byte[] _compressedData;
            private readonly PlayEndData _data;

            public UploadWithCookieRequestDescriptor(Replay replay, PlayEndData data) {
                _compressedData = CompressReplay(replay);
                _data = data;
            }

            public UnityWebRequest CreateWebRequest() {
                var query = new Dictionary<string, object>() {
                    { "type", (int)_data.EndType },
                    { "time", _data.Time }
                };
                var url = string.Format(WithCookieEndpoint, NetworkingUtils.ToHttpParams(query));
                var request = UnityWebRequest.Put(url, _compressedData);
                request.SetRequestHeader("Content-Encoding", "gzip");
                return request;
            }

            public Score ParseResponse(UnityWebRequest request) {
                return null;
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
