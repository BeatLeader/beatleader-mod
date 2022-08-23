using System.IO;
using System.IO.Compression;
using System.Text;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class UploadReplayRequest : PersistentSingletonRequestHandler<UploadReplayRequest, Player> {
        private const string WithCookieEndpoint = BLConstants.BEATLEADER_API_URL + "/replayoculus";
            
        public static void SendRequest(Replay replay) {
            var requestDescriptor = new UploadWithCookieRequestDescriptor(replay);
            instance.Send(requestDescriptor);
        }

        #region RequestDescriptor

        private class UploadWithCookieRequestDescriptor : IWebRequestDescriptor<Player> {
            private readonly byte[] _compressedData;

            public UploadWithCookieRequestDescriptor(Replay replay) {
                _compressedData = CompressReplay(replay);
            }

            public UnityWebRequest CreateWebRequest() {
                return UnityWebRequest.Put(WithCookieEndpoint, _compressedData);
            }

            public Player ParseResponse(UnityWebRequest request) {
                return JsonConvert.DeserializeObject<Player>(request.downloadHandler.text, NetworkingUtils.SerializerSettings);
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